using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Repository.Contract;
using MovieReservationSystem.Core.Service.Contract;
using MovieReservationSystem.Repository.Data;
using Stripe;

namespace MovieReservationSystem.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly IReservationRepository _reservationRepository;
        private readonly IShowtimeService _showtimeService;
        private readonly ISeatHoldService _seatHoldService;
        private readonly ISeatService _seatService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            AppDbContext context,
            IReservationRepository reservationRepository,
            IShowtimeService showtimeService,
            ISeatHoldService seatHoldService,
            ISeatService seatService,
            IConfiguration configuration,
            ILogger<PaymentService> logger)
        {
            _context = context;
            _reservationRepository = reservationRepository;
            _showtimeService = showtimeService;
            _seatHoldService = seatHoldService;
            _seatService = seatService;
            _configuration = configuration;
            _logger = logger;
            StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];
        }

        public async Task<Payment?> CreateOrUpdatePaymentIntentAsync(int reservationId)
        {
            var reservation = await _reservationRepository.GetReservationByIdAsync(reservationId);
            if (reservation == null) return null;
            if (reservation.Status != "Pending") return null;

            var showtime = await _showtimeService.GetShowtimeByIdAsync(reservation.ShowtimeId);
            if (showtime == null) return null;

            var currency = "usd";
            var amountDecimal = showtime.TicketPrice;
            var amountInCents = (long)Math.Round(amountDecimal * 100m);

            var paymentIntentService = new PaymentIntentService();

            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.ReservationId == reservationId);

            try
            {
                if (payment == null)
                {
                    var idempotencyKey = Guid.NewGuid().ToString();
                    var createOptions = new PaymentIntentCreateOptions
                    {
                        Amount = amountInCents,
                        Currency = currency,
                        PaymentMethodTypes = new List<string> { "card" },
                        Metadata = new Dictionary<string, string> { ["reservationId"] = reservationId.ToString() }
                    };

                    var requestOptions = new RequestOptions { IdempotencyKey = idempotencyKey };
                    var pi = await paymentIntentService.CreateAsync(createOptions, requestOptions);

                    var newPayment = new Payment
                    {
                        ReservationId = reservationId,
                        PaymentIntentId = pi.Id,
                        ClientSecret = pi.ClientSecret ?? string.Empty,
                        Amount = amountDecimal,
                        IdempotencyKey = idempotencyKey,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _context.Payments.AddAsync(newPayment);
                    await _context.SaveChangesAsync();

                    return newPayment;
                }
                else
                {
                    var updateOptions = new PaymentIntentUpdateOptions { Amount = amountInCents };
                    var requestOptions = new RequestOptions { IdempotencyKey = payment.IdempotencyKey };
                    await paymentIntentService.UpdateAsync(payment.PaymentIntentId, updateOptions, requestOptions);

                    payment.Amount = amountDecimal;
                    payment.UpdatedAt = DateTime.UtcNow;
                    _context.Payments.Update(payment);
                    await _context.SaveChangesAsync();

                    return payment;
                }
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating/updating PaymentIntent for reservation {ReservationId}", reservationId);
                throw;
            }
        }

        public async Task<Reservation?> CreatePaymentFromSeatHoldAsync(int showtimeId, int seatId, string userId, int holdMinutes = 5)
        {
            var activeHolds = await _seatHoldService.GetActiveHoldsAsync(showtimeId, new[] { seatId });
            var hold = activeHolds.FirstOrDefault(h => h.UserId == userId && h.SeatId == seatId);
            if (hold == null || hold.ExpiresAt <= DateTime.UtcNow)
                return null;

            if (await _reservationRepository.IsSeatReservedAsync(showtimeId, seatId))
                return null;

            var reservation = new Reservation
            {
                UserId = userId,
                ShowtimeId = showtimeId,
                SeatId = seatId,
                ReservationDate = DateTime.UtcNow,
                Status = "Pending"
            };

            await _reservationRepository.AddReservationAsync(reservation);
            await _reservationRepository.SaveChangesAsync();

            await CreateOrUpdatePaymentIntentAsync(reservation.Id);

            return reservation;
        }

        public async Task<Reservation?> UpdatePaymentStatusAsync(string paymentIntentId, bool isSucceeded)
        {
            if (string.IsNullOrWhiteSpace(paymentIntentId)) return null;

            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentIntentId == paymentIntentId);
            if (payment == null) return null;

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var reservation = await _reservationRepository.GetReservationByIdAsync(payment.ReservationId);
                if (reservation == null)
                {
                    await tx.RollbackAsync();
                    return null;
                }

                reservation.Status = isSucceeded ? "Confirmed" : "Canceled";
                await _reservationRepository.UpdateReservationAsync(reservation);

                var holds = await _seatHoldService.GetActiveHoldsAsync(reservation.ShowtimeId, new[] { reservation.SeatId });
                var userHold = holds.FirstOrDefault(h => h.SeatId == reservation.SeatId && h.UserId == reservation.UserId);
                if (userHold != null) await _seatHoldService.RemoveHoldsAsync(new[] { userHold });

                payment.UpdatedAt = DateTime.UtcNow;
                _context.Payments.Update(payment);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return reservation;
            }
            catch (DbUpdateException dbEx)
            {
                await tx.RollbackAsync();

                if (isSucceeded)
                {
                    try
                    {
                        var refundService = new RefundService();
                        var refundOptions = new RefundCreateOptions { PaymentIntent = paymentIntentId };
                        await refundService.CreateAsync(refundOptions);
                        _logger.LogWarning("Refund issued for PaymentIntent {PaymentIntentId} due to DB conflict", paymentIntentId);
                    }
                    catch (StripeException rex)
                    {
                        _logger.LogError(rex, "Failed to refund PaymentIntent {PaymentIntentId} after DB conflict", paymentIntentId);
                    }
                }

                _logger.LogError(dbEx, "DB update error while processing PaymentIntent {PaymentIntentId}", paymentIntentId);
                throw;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Unexpected error processing PaymentIntent {PaymentIntentId}", paymentIntentId);
                throw;
            }
        }

    }
}
