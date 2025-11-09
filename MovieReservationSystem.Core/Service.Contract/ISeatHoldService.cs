using MovieReservationSystem.Core.Entities;

namespace MovieReservationSystem.Core.Service.Contract
{
    public interface ISeatHoldService
    {
        Task<bool> IsSeatHeldAsync(int showtimeId, int seatId);
        Task<SeatHold> HoldSeatAsync(string userId, int showtimeId, int seatId, int holdMinutes = 5);
        Task ReleaseHoldAsync(string userId, int showtimeId, int seatId);
        Task RemoveHoldsAsync(IEnumerable<SeatHold> holds);
        Task RemoveExpiredHoldsAsync();
        Task<IEnumerable<SeatHold>> GetActiveHoldsAsync(int showtimeId, IEnumerable<int> seatIds);
        Task<IEnumerable<SeatHold>> GetHoldsByUserAsync(string userId);
    }
}
