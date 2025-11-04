using Microsoft.EntityFrameworkCore;
using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Interfaces;
using MovieReservationSystem.Repository.Data;

namespace MovieReservationSystem.Repository.Repositories
{
    public class SeatHoldRepository : GenericRepository<SeatHold, int>, ISeatHoldRepository
    {
        private readonly AppDbContext _context;

        public SeatHoldRepository(AppDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<bool> IsSeatHeldAsync(int showtimeId, int seatId)
        {
            return await _context.SeatHolds
                .AnyAsync(h => h.ShowtimeId == showtimeId &&
                               h.SeatId == seatId &&
                               h.ExpiresAt > DateTime.UtcNow);
        }

        public async Task AddHoldAsync(SeatHold hold)
        {
            await _context.SeatHolds.AddAsync(hold);
        }

        public async Task<IEnumerable<SeatHold>> GetActiveHoldsAsync(int showtimeId, IEnumerable<int> seatIds)
        {
            return await _context.SeatHolds
                .Where(h => h.ShowtimeId == showtimeId &&
                            seatIds.Contains(h.SeatId) &&
                            h.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<IEnumerable<SeatHold>> GetHoldsByUserAsync(string userId)
        {
            return await _context.SeatHolds
                .Where(h => h.UserId == userId && h.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task RemoveHoldsAsync(IEnumerable<SeatHold> holds)
        {
            _context.SeatHolds.RemoveRange(holds);
            await Task.CompletedTask;
        }

        public async Task RemoveExpiredHoldsAsync()
        {
            var expiredHolds = await _context.SeatHolds
                .Where(h => h.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();

            if (expiredHolds.Any())
                _context.SeatHolds.RemoveRange(expiredHolds);
        }

        public new async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
