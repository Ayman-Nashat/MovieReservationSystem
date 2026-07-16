using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Interfaces;
using MovieReservationSystem.Core.Repository.Contract;
using MovieReservationSystem.Repository.Data;

namespace MovieReservationSystem.Core.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly ISeatHoldRepository _seatHoldRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            IReservationRepository reservationRepository,
            ISeatHoldRepository seatHoldRepository,
            AppDbContext context,
            ILogger<ReservationService> logger)
        {
            _reservationRepository = reservationRepository;
            _seatHoldRepository = seatHoldRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<Reservation?> CreateReservationAsync(string userId, int showtimeId, int seatId)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));
            if (showtimeId <= 0)
                throw new ArgumentException("Showtime ID must be greater than 0.", nameof(showtimeId));
            if (seatId <= 0)
                throw new ArgumentException("Seat ID must be greater than 0.", nameof(seatId));

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Clean up expired holds first
                    await RemoveExpiredHoldsAsync();

                    // 2. Check if seat is already reserved
                    if (await _reservationRepository.IsSeatReservedAsync(showtimeId, seatId))
                    {
                        _logger.LogWarning("Seat {SeatId} for showtime {ShowtimeId} is already reserved.", seatId, showtimeId);
                        throw new InvalidOperationException("Seat is already reserved for this showtime.");
                    }

                    // 3. Check if seat is held by another user
                    var activeHolds = await _seatHoldRepository.GetActiveHoldsAsync(showtimeId, new[] { seatId });
                    var hold = activeHolds.FirstOrDefault();

                    if (hold != null && hold.UserId != userId)
                    {
                        _logger.LogWarning("Seat {SeatId} is held by user {HeldBy}. User {UserId} cannot reserve.",
                            seatId, hold.UserId, userId);
                        throw new InvalidOperationException("Seat is currently on hold by another user. Please select a different seat.");
                    }

                    // 4. Create reservation
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

                    // 5. Release the hold if it was held by this user
                    if (hold?.UserId == userId)
                    {
                        await _seatHoldRepository.RemoveHoldsAsync(new[] { hold });
                        await _seatHoldRepository.SaveChangesAsync();
                        _logger.LogInformation("Seat hold {HoldId} released upon reservation creation.", hold.Id);
                    }

                    await transaction.CommitAsync();
                    _logger.LogInformation("Reservation created for user {UserId}, seat {SeatId}, showtime {ShowtimeId}.",
                        userId, seatId, showtimeId);

                    return reservation;
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning("Unique constraint violation: Seat {SeatId} was just reserved.", seatId);
                    throw new InvalidOperationException("Seat was reserved by another user. Please select a different seat.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error creating reservation for user {UserId}.", userId);
                    throw;
                }
            }
        }

        public async Task<IEnumerable<Reservation>> GetUserReservationsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            return await _reservationRepository.GetReservationsByUserAsync(userId);
        }

        public async Task<IEnumerable<Reservation>> GetShowtimeReservationsAsync(int showtimeId)
        {
            if (showtimeId <= 0)
                throw new ArgumentException("Showtime ID must be greater than 0.", nameof(showtimeId));

            return await _reservationRepository.GetReservationsByShowtimeAsync(showtimeId);
        }

        public async Task<Reservation?> GetReservationByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Reservation ID must be greater than 0.", nameof(id));

            return await _reservationRepository.GetReservationByIdAsync(id);
        }

        public async Task ConfirmReservationAsync(int reservationId)
        {
            if (reservationId <= 0)
                throw new ArgumentException("Reservation ID must be greater than 0.", nameof(reservationId));

            var reservation = await _reservationRepository.GetReservationByIdAsync(reservationId);
            if (reservation == null)
                throw new KeyNotFoundException("Reservation not found.");

            if (reservation.Status == "Canceled")
                throw new InvalidOperationException("Cannot confirm a canceled reservation.");

            if (reservation.Status == "Confirmed")
                throw new InvalidOperationException("Reservation is already confirmed.");

            reservation.Status = "Confirmed";
            await _reservationRepository.UpdateReservationAsync(reservation);
            await _reservationRepository.SaveChangesAsync();

            _logger.LogInformation("Reservation {ReservationId} confirmed.", reservationId);
        }

        public async Task CancelReservationAsync(int reservationId)
        {
            if (reservationId <= 0)
                throw new ArgumentException("Reservation ID must be greater than 0.", nameof(reservationId));

            var reservation = await _reservationRepository.GetReservationByIdAsync(reservationId);
            if (reservation == null)
                throw new KeyNotFoundException("Reservation not found.");

            if (reservation.Status == "Canceled")
                throw new InvalidOperationException("Reservation is already canceled.");

            reservation.Status = "Canceled";
            await _reservationRepository.UpdateReservationAsync(reservation);
            await _reservationRepository.SaveChangesAsync();

            _logger.LogInformation("Reservation {ReservationId} canceled.", reservationId);
        }

        public async Task<bool> IsSeatReservedAsync(int showtimeId, int seatId)
        {
            if (showtimeId <= 0)
                throw new ArgumentException("Showtime ID must be greater than 0.", nameof(showtimeId));
            if (seatId <= 0)
                throw new ArgumentException("Seat ID must be greater than 0.", nameof(seatId));

            return await _reservationRepository.IsSeatReservedAsync(showtimeId, seatId);
        }

        /// <summary>
        /// Helper method to remove expired seat holds from the database
        /// </summary>
        private async Task RemoveExpiredHoldsAsync()
        {
            try
            {
                await _seatHoldRepository.RemoveExpiredHoldsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing expired holds during reservation creation.");
                // Don't throw - expired hold removal is non-critical
            }
        }
    }
}