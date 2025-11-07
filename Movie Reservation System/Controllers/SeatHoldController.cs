using Microsoft.AspNetCore.Mvc;
using Movie_Reservation_System.DTOs.Seathold;
using MovieReservationSystem.Core.Service.Contract;

namespace Movie_Reservation_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeatHoldController : ControllerBase
    {
        private readonly ISeatHoldService _seatHoldService;
        public SeatHoldController(ISeatHoldService seatHoldService)
        {
            _seatHoldService = seatHoldService;
        }

        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> HoldSeat([FromBody] HoldSeatRequest dto)
        {
            var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var hold = await _seatHoldService.HoldSeatAsync(userId, dto.ShowtimeId, dto.SeatId, dto.HoldMinutes);
                return Ok(new
                {
                    hold.Id,
                    hold.ShowtimeId,
                    hold.SeatId,
                    hold.UserId,
                    hold.ExpiresAt
                });
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

