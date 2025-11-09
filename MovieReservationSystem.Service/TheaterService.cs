using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Repository.Contract;
using MovieReservationSystem.Core.Service.Contract;
using MovieReservationSystem.Repository.Data;

namespace MovieReservationSystem.Service
{
    public class TheaterService : ITheaterService
    {
        private readonly ITheaterRepository _theaterRepository;
        private readonly AppDbContext _context;

        public TheaterService(ITheaterRepository theaterRepository, AppDbContext context)
        {
            _theaterRepository = theaterRepository;
            _context = context;
        }

        public async Task<IEnumerable<Theater>> GetAllTheatersAsync()
        {
            return await _theaterRepository.GetAllAsync();
        }

        public async Task<Theater?> GetTheaterByIdAsync(int id)
        {
            return await _theaterRepository.GetByIdAsync(id);
        }

        public async Task<Theater> AddTheaterAsync(Theater theater)
        {
            await _theaterRepository.AddAsync(theater);
            await _context.SaveChangesAsync();
            return theater;
        }

        public async Task<bool> UpdateTheaterAsync(Theater theater)
        {
            _theaterRepository.Update(theater);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTheaterAsync(int id)
        {
            var theater = await _theaterRepository.GetByIdAsync(id);
            if (theater == null)
                return false;

            _theaterRepository.Remove(theater);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task AddSeatsAsync(IEnumerable<Seat> seats)
        {
            await _context.Seats.AddRangeAsync(seats);
            await _context.SaveChangesAsync();
        }
    }
}
