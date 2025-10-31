using MovieReservationSystem.Core.Entities;

namespace MovieReservationSystem.Core.Service.Contract
{
    public interface IMovieService
    {
        Task<IEnumerable<Movie>> GetAllMoviesAsync();
        Task<Movie?> GetMovieByIdAsync(int id);
        Task<IEnumerable<Movie>> SearchByNameAsync(string name);
        //Task<IEnumerable<Movie>> SearchByGenreAsync(string genre);
        Task AddMovieAsync(Movie movie);
        Task UpdateMovieAsync(Movie movie);
        Task DeleteMovieAsync(int id);
    }
}
