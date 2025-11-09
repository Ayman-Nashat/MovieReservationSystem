using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Interfaces;
using MovieReservationSystem.Core.Repository.Contract;
using MovieReservationSystem.Core.Service.Contract;

namespace MovieReservationSystem.Service
{
    public class SeatHoldService : ISeatHoldService
    {
        private readonly ISeatHoldRepository _seatHoldRepository;
        private readonly IReservationRepository _reservationRepository;

        public SeatHoldService(
            ISeatHoldRepository seatHoldRepository,
            IReservationRepository reservationRepository)
        {
            _seatHoldRepository = seatHoldRepository;
            _reservationRepository = reservationRepository;
        }

        public async Task<bool> IsSeatHeldAsync(int showtimeId, int seatId)
        {
            return await _seatHoldRepository.IsSeatHeldAsync(showtimeId, seatId);
        }

        public async Task<SeatHold> HoldSeatAsync(string userId, int showtimeId, int seatId, int holdMinutes = 5)
        {
            // 1. Do not allow hold if seat already reserved
            if (await _reservationRepository.IsSeatReservedAsync(showtimeId, seatId))
                throw new InvalidOperationException("Seat is already reserved.");

            var now = DateTime.UtcNow;

            // 2. Check active holds for this seat
            var existingHolds = await _seatHoldRepository.GetActiveHoldsAsync(showtimeId, new[] { seatId });
            var existing = existingHolds.FirstOrDefault(h => h.SeatId == seatId);

            if (existing != null)
            {
                if (existing.UserId != userId)
                {
                    // held by someone else
                    throw new InvalidOperationException("Seat is currently on hold by another user.");
                }

                // held by same user -> refresh expiry
                existing.ExpiresAt = now.AddMinutes(holdMinutes);
                await _seatHoldRepository.SaveChangesAsync();
                return existing;
            }

            // 3. Create new hold
            var hold = new SeatHold
            {
                UserId = userId,
                ShowtimeId = showtimeId,
                SeatId = seatId,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(holdMinutes)
            };

            await _seatHoldRepository.AddHoldAsync(hold);
            await _seatHoldRepository.SaveChangesAsync();

            return hold;
        }

        public async Task ReleaseHoldAsync(string userId, int showtimeId, int seatId)
        {
            // find the active hold(s) for this user + seat and remove them
            var holds = await _seatHoldRepository.GetActiveHoldsAsync(showtimeId, new[] { seatId });
            var userHolds = holds.Where(h => h.UserId == userId).ToList();

            if (!userHolds.Any())
                return; // nothing to remove

            await _seatHoldRepository.RemoveHoldsAsync(userHolds);
            await _seatHoldRepository.SaveChangesAsync();
        }

        public async Task RemoveHoldsAsync(IEnumerable<SeatHold> holds)
        {
            if (holds == null) return;
            var list = holds.ToList();
            if (!list.Any()) return;

            await _seatHoldRepository.RemoveHoldsAsync(list);
            await _seatHoldRepository.SaveChangesAsync();
        }

        public async Task RemoveExpiredHoldsAsync()
        {
            await _seatHoldRepository.RemoveExpiredHoldsAsync();
            await _seatHoldRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<SeatHold>> GetActiveHoldsAsync(int showtimeId, IEnumerable<int> seatIds)
        {
            return await _seatHoldRepository.GetActiveHoldsAsync(showtimeId, seatIds);
        }

        public async Task<IEnumerable<SeatHold>> GetHoldsByUserAsync(string userId)
        {
            return await _seatHoldRepository.GetHoldsByUserAsync(userId);
        }
    }

}
