using MovieReservationSystem.Core.Entities;

namespace MovieReservationSystem.Core.Repository.Contract
{
    public interface ISeatRepository : IGenericRepository<Seat, int>
    {
        Task<Seat?> GetByPositionAsync(int seatId);
        Task<IEnumerable<Seat>> GetSeatsByTheaterIdAsync(int theaterId);
    }
}
