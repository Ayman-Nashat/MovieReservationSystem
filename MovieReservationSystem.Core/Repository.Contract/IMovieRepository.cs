using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Repository.Contract;

namespace MovieReservationSystem.Core.Repositort.Contract
{
    public interface IMovieRepository : IGenericRepository<Movie, int>
    {
        Task<Movie?> GetMovieWithDetailsAsync(int id);
        Task<IEnumerable<Movie>> GetAllMoviesWithDetailsAsync();
        Task AddMovieWithTheaterAndShowtimesAsync(Movie movie, Theater theater, List<Showtime> showtimes);
        Task<IEnumerable<Movie>> SearchByNameAsync(string name);
        //Task<IEnumerable<Movie>> GetByGenreAsync(string genre);
        Task<int> SaveChangesAsync();
        void Delete(Movie movie);
    }
}
