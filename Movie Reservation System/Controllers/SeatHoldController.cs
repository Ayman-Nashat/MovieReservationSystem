using Microsoft.AspNetCore.Mvc;
using Movie_Reservation_System.DTOs.Seathold;
using MovieReservationSystem.Core.Interfaces;
using MovieReservationSystem.Core.Service.Contract;

namespace Movie_Reservation_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeatHoldController : ControllerBase
    {
        private readonly ISeatHoldService _seatHoldService;
        private readonly IShowtimeService _showtimeService;
        private readonly ISeatService _seatService;
        private readonly IReservationService _reservationService;

        public SeatHoldController(ISeatHoldService seatHoldService, IShowtimeService showtimeService, ISeatService seatService, IReservationService reservationService)
        {
            _seatHoldService = seatHoldService;
            _showtimeService = showtimeService;
            _seatService = seatService;
            _reservationService = reservationService;
        }

        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> HoldSeat([FromBody] HoldSeatRequest dto)
        {
            var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (dto == null || dto.ShowtimeId <= 0 || dto.SeatId == null)
                return BadRequest("showtimeId and seatId are required.");

            int seatId = dto.SeatId.Value;

            var showtime = await _showtimeService.GetShowtimeByIdAsync(dto.ShowtimeId);
            if (showtime == null) return NotFound("Showtime not found.");

            var seat = await _seatService.GetByPositionAsync(seatId);
            if (seat == null) return NotFound("Seat not found.");

            if (seat.TheaterId != showtime.TheaterId)
                return BadRequest(new { message = "Seat does not belong to the showtime's theater." });

            if (await _reservationService.IsSeatReservedAsync(dto.ShowtimeId, seatId))
                return Conflict(new { message = "Seat is already reserved for this showtime." });

            var isHeld = await _seatHoldService.IsSeatHeldAsync(dto.ShowtimeId, seatId);
            if (isHeld)
            {
                return Conflict(new { message = "Seat is currently on hold by another user." });
            }

            try
            {
                var hold = await _seatHoldService.HoldSeatAsync(userId, dto.ShowtimeId, seatId, dto.HoldMinutes);
                return Ok(new { hold.Id, hold.ShowtimeId, hold.SeatId, hold.UserId, hold.ExpiresAt });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }



        [HttpDelete]
        //[Authorize]
        public async Task<IActionResult> ReleaseHold([FromBody] ReleaseSeatRequest dto)
        {
            var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _seatHoldService.ReleaseHoldAsync(userId, dto.ShowtimeId, dto.SeatId);
            return NoContent();
        }

        [HttpGet("showtime/{showtimeId}")]
        public async Task<IActionResult> GetActiveHoldsForSeats(int showtimeId, [FromQuery] string seatIds)
        {
            IEnumerable<int> ids = Enumerable.Empty<int>();
            if (!string.IsNullOrWhiteSpace(seatIds))
            {
                ids = seatIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(s => int.TryParse(s.Trim(), out var v) ? v : (int?)null)
                             .Where(v => v.HasValue)
                             .Select(v => v!.Value);
            }

            var holds = await _seatHoldService.GetActiveHoldsAsync(showtimeId, ids);
            var result = holds.Select(h => new { h.SeatId, h.UserId, h.ExpiresAt });
            return Ok(result);
        }
        [HttpGet("user")]
        //[Authorize]
        public async Task<IActionResult> GetUserHolds()
        {
            var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var holds = await _seatHoldService.GetHoldsByUserAsync(userId);
            var result = holds.Select(h => new { h.Id, h.ShowtimeId, h.SeatId, h.ExpiresAt });
            return Ok(result);
        }

        [HttpDelete("removeExpired")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveExpired()
        {
            await _seatHoldService.RemoveExpiredHoldsAsync();
            return Ok();
        }
    }
}

