using MovieReservationSystem.Core.Entities;

namespace MovieReservationSystem.Core.Service.Contract
{
    public interface IShowtimeService
    {
        Task<IEnumerable<Showtime>> GetAllShowtimesAsync();
        Task<Showtime?> GetShowtimeByIdAsync(int id);
        Task<Showtime> AddShowtimeAsync(Showtime showtime);
        Task<Showtime?> UpdateShowtimeAsync(Showtime showtime);
        Task<bool> DeleteShowtimeAsync(int id);
    }
}
