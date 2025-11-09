using Microsoft.EntityFrameworkCore;
using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Repository.Contract;
using MovieReservationSystem.Repository.Data;

namespace MovieReservationSystem.Repository.Repositories
{
    public class SeatRepository : GenericRepository<Seat, int>, ISeatRepository
    {
        private readonly AppDbContext _context;

        public SeatRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<Seat?> GetByPositionAsync(int seatId)
        {
            return await _context.Seats
                .FirstOrDefaultAsync(s => s.Id == seatId);
        }
        public async Task<IEnumerable<Seat>> GetSeatsByTheaterIdAsync(int theaterId)
        {
            return await _context.Seats
                .Where(s => s.TheaterId == theaterId)
                .OrderBy(s => s.RowNumber)
                .ThenBy(s => s.ColumnNumber)
                .ToListAsync();
        }
    }
}
