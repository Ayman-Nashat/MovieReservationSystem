using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Interfaces;
using MovieReservationSystem.Core.Repository.Contract;

namespace MovieReservationSystem.Core.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly ISeatHoldRepository _seatHoldRepository;

        public ReservationService(
            IReservationRepository reservationRepository,
            ISeatHoldRepository seatHoldRepository)
        {
            _reservationRepository = reservationRepository;
            _seatHoldRepository = seatHoldRepository;
        }

        public async Task<Reservation?> CreateReservationAsync(string userId, int showtimeId, int seatId)
        {
            if (await _reservationRepository.IsSeatReservedAsync(showtimeId, seatId))
                throw new InvalidOperationException("Seat is already reserved.");

            if (await _seatHoldRepository.IsSeatHeldAsync(showtimeId, seatId))
                throw new InvalidOperationException("Seat is currently on hold.");

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

            return reservation;
        }

        public async Task<IEnumerable<Reservation>> GetUserReservationsAsync(string userId)
        {
            return await _reservationRepository.GetReservationsByUserAsync(userId);
        }

        public async Task<IEnumerable<Reservation>> GetShowtimeReservationsAsync(int showtimeId)
        {
            return await _reservationRepository.GetReservationsByShowtimeAsync(showtimeId);
        }

        public async Task<Reservation?> GetReservationByIdAsync(int id)
        {
            return await _reservationRepository.GetReservationByIdAsync(id);
        }

        public async Task ConfirmReservationAsync(int reservationId)
        {
            var reservation = await _reservationRepository.GetReservationByIdAsync(reservationId);
            if (reservation == null)
                throw new KeyNotFoundException("Reservation not found.");

            reservation.Status = "Confirmed";
            await _reservationRepository.UpdateReservationAsync(reservation);
            await _reservationRepository.SaveChangesAsync();
        }

        public async Task CancelReservationAsync(int reservationId)
        {
            var reservation = await _reservationRepository.GetReservationByIdAsync(reservationId);
            if (reservation == null)
                throw new KeyNotFoundException("Reservation not found.");

            reservation.Status = "Canceled";
            await _reservationRepository.UpdateReservationAsync(reservation);
            await _reservationRepository.SaveChangesAsync();
        }

        public async Task<bool> IsSeatReservedAsync(int showtimeId, int seatId)
        {
            return await _reservationRepository.IsSeatReservedAsync(showtimeId, seatId);
        }

    }
}
