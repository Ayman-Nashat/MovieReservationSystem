using Microsoft.EntityFrameworkCore;
using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Interfaces;
using MovieReservationSystem.Repository.Data;

namespace MovieReservationSystem.Repository.Repositories
{
    public class SeatHoldRepository : ISeatHoldRepository
    {
        private readonly AppDbContext _context;

        public SeatHoldRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsSeatHeldAsync(int showtimeId, int seatId)
        {
            return await _context.SeatHolds
                .AnyAsync(h => h.ShowtimeId == showtimeId && h.SeatId == seatId && h.ExpiresAt > DateTime.UtcNow);
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

        public async Task RemoveHoldsAsync(IEnumerable<SeatHold> holds)
        {
            _context.SeatHolds.RemoveRange(holds);
            await Task.CompletedTask;
        }

        public async Task RemoveExpiredHoldsAsync()
        {
            var expired = await _context.SeatHolds
                .Where(h => h.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();

            _context.SeatHolds.RemoveRange(expired);
        }

        public async Task<IEnumerable<SeatHold>> GetHoldsByUserAsync(string userId)
        {
            return await _context.SeatHolds
                .Where(h => h.UserId == userId && h.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
