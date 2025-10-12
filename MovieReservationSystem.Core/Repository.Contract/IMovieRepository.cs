using MovieReservationSystem.Core.Entities;

namespace MovieReservationSystem.Core.Repositort.Contract
{
    public interface IMovieRepository
    {
        Task<IEnumerable<Movie>> GetAllAsync();
        Task<Movie?> GetByIdAsync(int id);
        Task<IEnumerable<Movie>> GetByNameAsync(string name);
        Task<IEnumerable<Movie>> GetByGenreAsync(string genre); Task AddAsync(Movie movie);
        void Update(Movie movie);
        void Delete(Movie movie);
        Task SaveChangesAsync();

    }
}
