using MovieReservationSystem.Core.Entities;

namespace MovieReservationSystem.Core.Interfaces
{
    public interface ISeatHoldRepository
    {
        Task<bool> IsSeatHeldAsync(int showtimeId, int seatId);
        Task AddHoldAsync(SeatHold hold);
        Task<IEnumerable<SeatHold>> GetActiveHoldsAsync(int showtimeId, IEnumerable<int> seatIds);
        Task RemoveHoldsAsync(IEnumerable<SeatHold> holds);
        Task RemoveExpiredHoldsAsync();
        Task<IEnumerable<SeatHold>> GetHoldsByUserAsync(string userId);
        Task SaveChangesAsync();
        Task<Seat?> GetByPositionAsync(int theaterId, int row, int column);
    }
}
