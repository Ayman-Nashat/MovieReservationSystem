using MovieReservationSystem.Core.Entities;

namespace MovieReservationSystem.Core.Repository.Contract
{
    public interface IReservationRepository
    {
        Task<bool> IsSeatReservedAsync(int showtimeId, int seatId);
        Task AddReservationAsync(Reservation reservation);
        Task<IEnumerable<Reservation>> GetReservationsByShowtimeAsync(int showtimeId);
        Task<IEnumerable<Reservation>> GetReservationsByUserAsync(string userId);
        Task<Reservation?> GetReservationByIdAsync(int reservationId);
        Task UpdateReservationAsync(Reservation reservation);
        Task DeleteReservationAsync(int reservationId);
        Task SaveChangesAsync();
    }
}
