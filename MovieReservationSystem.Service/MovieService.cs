using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Repositort.Contract;
using MovieReservationSystem.Core.Service.Contract;

namespace MovieReservationSystem.Service
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;

        public MovieService(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        public async Task<IEnumerable<Movie>> GetAllMoviesAsync()
            => await _movieRepository.GetAllAsync();

        public async Task<Movie?> GetMovieByIdAsync(int id)
            => await _movieRepository.GetByIdAsync(id);

        public async Task<IEnumerable<Movie>> SearchByNameAsync(string name)
            => await _movieRepository.SearchByNameAsync(name);

        //public async Task<IEnumerable<Movie>> SearchByGenreAsync(string genre)
        //    => await _movieRepository.GetByGenreAsync(genre);

        public async Task AddMovieAsync(Movie movie)
        {
            await _movieRepository.AddAsync(movie);
            await _movieRepository.SaveChangesAsync();
        }

        public async Task UpdateMovieAsync(Movie movie)
        {
            _movieRepository.Update(movie);
            await _movieRepository.SaveChangesAsync();
        }

        public async Task DeleteMovieAsync(int id)
        {
            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie != null)
            {
                _movieRepository.Delete(movie);
                await _movieRepository.SaveChangesAsync();
            }
        }
    }
}
