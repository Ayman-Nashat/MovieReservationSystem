using Microsoft.EntityFrameworkCore;
using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Repositort.Contract;
using MovieReservationSystem.Repository.Data;

namespace MovieReservationSystem.Repository.Repositories
{
    public class MovieRepository : GenericRepository<Movie, int>, IMovieRepository
    {
        private readonly AppDbContext _context;

        public MovieRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Movie?> GetMovieWithDetailsAsync(int id)
        {
            return await _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.Showtimes).ThenInclude(s => s.Theater)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Movie>> GetAllMoviesWithDetailsAsync()
        {
            return await _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.Showtimes).ThenInclude(s => s.Theater)
                .ToListAsync();
        }

        public async Task AddMovieWithTheaterAndShowtimesAsync(Movie movie, Theater theater, List<Showtime> showtimes)
        {
            // Attach theater and showtimes
            _context.Theaters.Add(theater);
            await _context.SaveChangesAsync();

            movie.Showtimes = showtimes;
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Movie>> SearchByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Enumerable.Empty<Movie>();

            name = name.Trim().ToLower();

            return await _context.Movies
                .Where(m => m.Title.ToLower().Contains(name))
                .Include(m => m.Showtimes)
                .ToListAsync();
        }

        //public async Task<IEnumerable<Movie>> GetByGenreAsync(string genre)
        //{
        //    if (string.IsNullOrWhiteSpace(genre))
        //        return Enumerable.Empty<Movie>();

        //    genre = genre.Trim().ToLower();

        //    return await _context.Movies
        //        .Where(m => m.ToLower().Contains(genre))
        //        .Include(m => m.Showtimes)
        //        .ToListAsync();
        //}

        public async Task<int> SaveChangesAsync() =>
            await _context.SaveChangesAsync();

        public void Delete(Movie movie) =>
            _context.Movies.Remove(movie);
    }
}
