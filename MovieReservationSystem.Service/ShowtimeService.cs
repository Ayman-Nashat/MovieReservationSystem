using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Repositort.Contract;
using MovieReservationSystem.Core.Repository.Contract;
using MovieReservationSystem.Core.Service.Contract;

namespace MovieReservationSystem.Service
{
    public class ShowtimeService : IShowtimeService
    {
        private readonly IShowtimeRepository _showtimeRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly ITheaterRepository _theaterRepository;

        public ShowtimeService(
            IShowtimeRepository showtimeRepository,
            IMovieRepository movieRepository,
            ITheaterRepository theaterRepository)
        {
            _showtimeRepository = showtimeRepository;
            _movieRepository = movieRepository;
            _theaterRepository = theaterRepository;
        }

        public async Task<IEnumerable<Showtime>> GetAllShowtimesAsync()
        {
            return await _showtimeRepository.GetAllWithDetailsAsync();
        }

        public async Task<Showtime?> GetShowtimeByIdAsync(int id)
        {
            return await _showtimeRepository.GetByIdAsync(id);
        }

        public async Task<Showtime> AddShowtimeAsync(Showtime showtime)
        {
            showtime.StartTime = showtime.StartTime.ToUniversalTime();
            showtime.EndTime = showtime.EndTime.ToUniversalTime();

            var movie = await _movieRepository.GetByIdAsync(showtime.MovieId);
            var theater = await _theaterRepository.GetByIdAsync(showtime.TheaterId);
            if (movie == null || theater == null) throw new Exception("Invalid MovieId or TheaterId.");
            if (showtime.EndTime <= showtime.StartTime) throw new Exception("EndTime must be after StartTime.");
            if (!theater.IsActive) throw new Exception("Cannot add showtime to an inactive theater.");

            int bufferMinutes = 45;
            var startWithBuffer = showtime.StartTime.AddMinutes(-bufferMinutes);
            var endWithBuffer = showtime.EndTime.AddMinutes(bufferMinutes);

            bool isBusy = await _showtimeRepository.IsTheaterBusyAsync(
                showtime.TheaterId, startWithBuffer, endWithBuffer);

            if (isBusy)
                throw new Exception("The selected theater is busy during this time.");

            await _showtimeRepository.AddAsync(showtime);
            await _showtimeRepository.SaveChangesAsync();

            return showtime;
        }



        public async Task<Showtime?> UpdateShowtimeAsync(Showtime showtime)
        {
            var existing = await _showtimeRepository.GetByIdAsync(showtime.Id);
            if (existing == null)
                return null;

            existing.MovieId = showtime.MovieId;
            existing.TheaterId = showtime.TheaterId;
            existing.StartTime = showtime.StartTime;
            existing.EndTime = showtime.EndTime;
            existing.TicketPrice = showtime.TicketPrice;

            _showtimeRepository.Update(existing);
            await _showtimeRepository.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteShowtimeAsync(int id)
        {
            var showtime = await _showtimeRepository.GetByIdAsync(id);
            if (showtime == null)
                return false;

            _showtimeRepository.Remove(showtime);
            await _showtimeRepository.SaveChangesAsync();

            return true;
        }
    }
}
