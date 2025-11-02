using Microsoft.EntityFrameworkCore;
using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Repository.Contract;
using MovieReservationSystem.Repository.Data;

namespace MovieReservationSystem.Repository.Repositories
{
    public class ShowtimeRepository : GenericRepository<Showtime, int>, IShowtimeRepository
    {
        private readonly AppDbContext _context;

        public ShowtimeRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Showtime>> GetShowtimesByMovieIdAsync(int movieId)
        {
            return await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Theater)
                .Where(s => s.MovieId == movieId)
                .ToListAsync();
        }

        public async Task<bool> IsTheaterBusyAsync(int theaterId, DateTime startTimeUtc, DateTime endTimeUtc, int? excludeShowtimeId = null)
        {
            var query = _context.Showtimes
                .Where(s => s.TheaterId == theaterId
                         && s.StartTime < endTimeUtc
                         && s.EndTime > startTimeUtc
                         && s.Theater.IsActive
                );

            if (excludeShowtimeId.HasValue)
                query = query.Where(s => s.Id != excludeShowtimeId.Value);

            return await query.AnyAsync();
        }

    }
}
