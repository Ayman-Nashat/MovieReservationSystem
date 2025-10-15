using Microsoft.EntityFrameworkCore;
using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Repositort.Contract;
using MovieReservationSystem.Repository.Data;

namespace MovieReservationSystem.Repository.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly AppDbContext _context;

        public MovieRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Movie movie)
        => await _context.Movies.AddAsync(movie);

        public void Delete(Movie movie)
        => _context.Movies.Remove(movie);

        public async Task<IEnumerable<Movie>> GetAllAsync()
        {
            return await _context.Movies
                .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
                .ToListAsync();
        }

        public async Task<Movie?> GetByIdAsync(int id)
        {
            return await _context.Movies
                .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Movie>> GetByGenreAsync(string genre)
        => await _context.Movies
                .Where(m => m.MovieGenres.Any(mg => mg.Genre.Name.ToLower().Contains(genre.ToLower())))
                .ToListAsync();

        public async Task<IEnumerable<Movie>> GetByNameAsync(string name)
        => await _context.Movies
                .Where(m => m.Title.ToLower().Contains(name.ToLower()))
                .ToListAsync();

        public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

        public void Update(Movie movie)
        => _context.Movies.Update(movie);
    }
}
