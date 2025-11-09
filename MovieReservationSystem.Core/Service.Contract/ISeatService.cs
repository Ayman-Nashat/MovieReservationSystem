using MovieReservationSystem.Core.Entities;

namespace MovieReservationSystem.Core.Service.Contract
{
    public interface ISeatService
    {
        Task<Seat?> GetByPositionAsync(int seatId);
        Task<IEnumerable<Seat>> GetSeatsByTheaterIdAsync(int theaterId);
    }
}
