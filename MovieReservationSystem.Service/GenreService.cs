using Microsoft.EntityFrameworkCore;
using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Repository.Contract;
using MovieReservationSystem.Core.Service.Contract;
using MovieReservationSystem.Repository.Data;

namespace MovieReservationSystem.Service
{
    public class GenreService : IGenreService
    {
        private readonly IGenericRepository<Genre, int> _genreRepository;
        private readonly AppDbContext _context; // For SaveChangesAsync()

        public GenreService(IGenericRepository<Genre, int> genreRepository, AppDbContext context)
        {
            _genreRepository = genreRepository;
            _context = context;
        }

        public async Task<IEnumerable<Genre>> GetAllAsync()
        {
            return await _genreRepository.GetAllAsync();
        }

        public async Task<Genre?> GetByIdAsync(int id)
        {
            return await _genreRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(Genre genre)
        {
            await _genreRepository.AddAsync(genre);
            await _context.SaveChangesAsync();
        }

        public void Update(Genre genre)
        {
            _genreRepository.Update(genre);
            _context.SaveChangesAsync();
        }

        public void Delete(Genre genre)
        {
            _genreRepository.Remove(genre);
            _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AddGenreToMovieAsync(int genreId, int movieId)
        {
            var genre = await _context.Genres.FindAsync(genreId);
            var movie = await _context.Movies
                .Include(m => m.MovieGenres)
                .FirstOrDefaultAsync(m => m.Id == movieId);

            if (genre == null || movie == null)
                return false;

            if (movie.MovieGenres.Any(mg => mg.GenreId == genreId))
                return false;

            movie.MovieGenres.Add(new MovieGenre { GenreId = genreId, MovieId = movieId });
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
