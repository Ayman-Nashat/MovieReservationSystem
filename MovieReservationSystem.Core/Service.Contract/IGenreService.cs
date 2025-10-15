using MovieReservationSystem.Core.Entities;

namespace MovieReservationSystem.Core.Service.Contract
{
    public interface IGenreService
    {
        Task<IEnumerable<Genre>> GetAllAsync();
        Task<Genre?> GetByIdAsync(int id);
        Task AddAsync(Genre genre);
        void Update(Genre genre);
        void Delete(Genre genre);
        Task SaveChangesAsync();
        Task<bool> AddGenreToMovieAsync(int genreId, int movieId);

    }
}
