using Microsoft.EntityFrameworkCore;
using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Repository.Contract;
using MovieReservationSystem.Repository.Data;

namespace MovieReservationSystem.Repository.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly AppDbContext _context;

        public ReservationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsSeatReservedAsync(int showtimeId, int seatId)
        {
            return await _context.Reservations
                .AnyAsync(r => r.ShowtimeId == showtimeId && r.SeatId == seatId);
        }

        public async Task AddReservationAsync(Reservation reservation)
        {
            await _context.Reservations.AddAsync(reservation);
        }

        public async Task<IEnumerable<Reservation>> GetReservationsByShowtimeAsync(int showtimeId)
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Seat)
                .Include(r => r.Showtime)
                    .ThenInclude(s => s.Movie)
                .Where(r => r.ShowtimeId == showtimeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetReservationsByUserAsync(string userId)
        {
            return await _context.Reservations
                .Include(r => r.Seat)
                .Include(r => r.Showtime)
                    .ThenInclude(s => s.Movie)
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }

        public async Task<Reservation?> GetReservationByIdAsync(int reservationId)
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Seat)
                .Include(r => r.Showtime)
                    .ThenInclude(s => s.Movie)
                .FirstOrDefaultAsync(r => r.Id == reservationId);
        }

        public async Task UpdateReservationAsync(Reservation reservation)
        {
            _context.Reservations.Update(reservation);
            await Task.CompletedTask;
        }

        public async Task DeleteReservationAsync(int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation != null)
                _context.Reservations.Remove(reservation);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
