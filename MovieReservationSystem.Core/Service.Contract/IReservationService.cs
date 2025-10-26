using MovieReservationSystem.Core.Entities;

namespace MovieReservationSystem.Core.Interfaces
{
    public interface IReservationService
    {
        Task<Reservation?> CreateReservationAsync(string userId, int showtimeId, int seatId);
        Task<IEnumerable<Reservation>> GetUserReservationsAsync(string userId);
        Task<IEnumerable<Reservation>> GetShowtimeReservationsAsync(int showtimeId);
        Task<Reservation?> GetReservationByIdAsync(int id);
        Task ConfirmReservationAsync(int reservationId);
        Task CancelReservationAsync(int reservationId);
    }
}
