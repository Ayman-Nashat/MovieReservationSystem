using MovieReservationSystem.Core.Entities;

namespace MovieReservationSystem.Core.Repository.Contract
{
    public interface IShowtimeRepository : IGenericRepository<Showtime, int>
    {
        public Task<bool> IsTheaterBusyAsync(int theaterId, DateTime startTimeUtc, DateTime endTimeUtc, int? excludeShowtimeId = null);
        public Task<IEnumerable<Showtime>> GetShowtimesByMovieIdAsync(int movieId);

    }
}
