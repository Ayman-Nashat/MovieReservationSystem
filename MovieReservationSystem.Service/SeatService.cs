using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Repository.Contract;
using MovieReservationSystem.Core.Service.Contract;

namespace MovieReservationSystem.Service
{
    public class SeatService : ISeatService
    {
        private readonly ISeatRepository _seatRepository;

        public SeatService(ISeatRepository seatRepository)
        {
            _seatRepository = seatRepository;
        }

        public async Task<Seat?> GetByIdAsync(int id)
        {
            return await _seatRepository.GetByIdAsync(id);
        }

        public async Task<Seat?> GetByPositionAsync(int seatId)
        {
            return await _seatRepository.GetByPositionAsync(seatId);
        }

        public async Task<IEnumerable<Seat>> GetSeatsByTheaterIdAsync(int theaterId)
        {
            return await _seatRepository.GetSeatsByTheaterIdAsync(theaterId);
        }

        public async Task UpdateSeatAsync(Seat seat)
        {
            _seatRepository.Update(seat);
            await _seatRepository.SaveChangesAsync();
        }
    }
}
