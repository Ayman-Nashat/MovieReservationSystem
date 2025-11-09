using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Repository.Contract;

namespace MovieReservationSystem.Core.Interfaces
{
    public interface ISeatHoldRepository : IGenericRepository<SeatHold, int>
    {
        Task<bool> IsSeatHeldAsync(int showtimeId, int seatId);
        Task AddHoldAsync(SeatHold hold);
        Task<IEnumerable<SeatHold>> GetActiveHoldsAsync(int showtimeId, IEnumerable<int> seatIds);
        Task RemoveHoldsAsync(IEnumerable<SeatHold> holds);
        Task RemoveExpiredHoldsAsync();
        Task<IEnumerable<SeatHold>> GetHoldsByUserAsync(string userId);
    }
}
