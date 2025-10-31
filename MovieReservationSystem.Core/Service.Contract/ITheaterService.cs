using MovieReservationSystem.Core.Entities;

namespace MovieReservationSystem.Core.Service.Contract
{
    public interface ITheaterService
    {
        Task<IEnumerable<Theater>> GetAllTheatersAsync();
        Task<Theater?> GetTheaterByIdAsync(int id);
        Task<Theater> AddTheaterAsync(Theater theater);
        Task<bool> UpdateTheaterAsync(Theater theater);
        Task<bool> DeleteTheaterAsync(int id);
    }
}
