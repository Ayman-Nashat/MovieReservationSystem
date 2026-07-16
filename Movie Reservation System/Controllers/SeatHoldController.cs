using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movie_Reservation_System.DTOs.Seathold;
using MovieReservationSystem.Core.Interfaces;
using MovieReservationSystem.Core.Service.Contract;
using System.Security.Claims;

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
        private readonly ILogger<SeatHoldController> _logger;

        public SeatHoldController(
            ISeatHoldService seatHoldService,
            IShowtimeService showtimeService,
            ISeatService seatService,
            IReservationService reservationService,
            ILogger<SeatHoldController> logger)
        {
            _seatHoldService = seatHoldService;
            _showtimeService = showtimeService;
            _seatService = seatService;
            _reservationService = reservationService;
            _logger = logger;
        }

        /// <summary>
        /// Hold a seat for a showtime. Hold expires after specified minutes.
        /// If user already holds the seat, expiry time is refreshed.
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> HoldSeat([FromBody] HoldSeatRequest dto)
        {
            try
            {
                // Validate DTO
                if (dto == null)
                    return BadRequest(new { message = "Invalid request data." });

                if (dto.ShowtimeId <= 0)
                    return BadRequest(new { message = "Showtime ID must be greater than 0." });

                if (dto.SeatId == null || dto.SeatId <= 0)
                    return BadRequest(new { message = "Seat ID is required and must be greater than 0." });

                // ✅ Extract UserId from JWT token securely
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated." });

                int seatId = dto.SeatId.Value;
                int holdMinutes = dto.HoldMinutes > 0 ? dto.HoldMinutes : 5;

                _logger.LogInformation("User {UserId} requesting to hold seat {SeatId} for showtime {ShowtimeId}.",
                    userId, seatId, dto.ShowtimeId);

                // Validate showtime exists
                var showtime = await _showtimeService.GetShowtimeByIdAsync(dto.ShowtimeId);
                if (showtime == null)
                {
                    _logger.LogWarning("Showtime {ShowtimeId} not found.", dto.ShowtimeId);
                    return NotFound(new { message = "Showtime not found." });
                }

                // Validate seat exists
                var seat = await _seatService.GetByIdAsync(seatId);
                if (seat == null)
                {
                    _logger.LogWarning("Seat {SeatId} not found.", seatId);
                    return NotFound(new { message = "Seat not found." });
                }

                // Validate seat belongs to showtime's theater
                if (seat.TheaterId != showtime.TheaterId)
                {
                    _logger.LogWarning("Seat {SeatId} does not belong to theater {TheaterId}.", seatId, showtime.TheaterId);
                    return BadRequest(new { message = "Seat does not belong to the theater for this showtime." });
                }

                // Check if seat is already reserved
                if (await _reservationService.IsSeatReservedAsync(dto.ShowtimeId, seatId))
                {
                    _logger.LogWarning("Seat {SeatId} is already reserved for showtime {ShowtimeId}.", seatId, dto.ShowtimeId);
                    return Conflict(new { message = "Seat is already reserved for this showtime." });
                }

                // Attempt to hold the seat
                var hold = await _seatHoldService.HoldSeatAsync(userId, dto.ShowtimeId, seatId, holdMinutes);

                _logger.LogInformation("Seat {SeatId} held for user {UserId} until {ExpiresAt}.",
                    seatId, userId, hold.ExpiresAt);

                return Ok(new
                {
                    hold.Id,
                    hold.ShowtimeId,
                    hold.SeatId,
                    hold.UserId,
                    hold.ExpiresAt,
                    ExpiresIn = (hold.ExpiresAt - DateTime.UtcNow).TotalSeconds
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot hold seat: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid argument: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error holding seat.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while holding the seat." });
            }
        }

        /// <summary>
        /// Release a seat hold manually.
        /// Only the user who holds the seat can release it.
        /// </summary>
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> ReleaseHold([FromBody] ReleaseSeatRequest dto)
        {
            try
            {
                // Validate DTO
                if (dto == null)
                    return BadRequest(new { message = "Invalid request data." });

                if (dto.ShowtimeId <= 0)
                    return BadRequest(new { message = "Showtime ID must be greater than 0." });

                if (dto.SeatId <= 0)
                    return BadRequest(new { message = "Seat ID must be greater than 0." });

                // ✅ Extract UserId from JWT token securely
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated." });

                _logger.LogInformation("User {UserId} releasing hold on seat {SeatId} for showtime {ShowtimeId}.",
                    userId, dto.SeatId, dto.ShowtimeId);

                await _seatHoldService.ReleaseHoldAsync(userId, dto.ShowtimeId, dto.SeatId);

                _logger.LogInformation("Hold released for user {UserId} on seat {SeatId}.", userId, dto.SeatId);

                return NoContent(); // 204 No Content
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot release hold: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid argument: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error releasing hold.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while releasing the hold." });
            }
        }

        /// <summary>
        /// Get all active seat holds for a specific showtime and optional seat IDs.
        /// Only admins can view all holds. Regular users can only see their own holds.
        /// </summary>
        [HttpGet("showtime/{showtimeId}")]
        [Authorize]
        public async Task<IActionResult> GetActiveHoldsForSeats(int showtimeId, [FromQuery] string? seatIds)
        {
            try
            {
                if (showtimeId <= 0)
                    return BadRequest(new { message = "Invalid showtime ID." });

                // Parse seat IDs if provided
                IEnumerable<int> ids = Enumerable.Empty<int>();
                if (!string.IsNullOrWhiteSpace(seatIds))
                {
                    ids = seatIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => int.TryParse(s.Trim(), out var v) ? v : (int?)null)
                        .Where(v => v.HasValue)
                        .Select(v => v!.Value);
                }

                var holds = await _seatHoldService.GetActiveHoldsAsync(showtimeId, ids);

                // ✅ Authorization: Only admins see all holds, users see their own
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!User.IsInRole("Admin") && userId != null)
                {
                    holds = holds.Where(h => h.UserId == userId);
                    _logger.LogInformation("User {UserId} retrieving their own holds for showtime {ShowtimeId}.", userId, showtimeId);
                }
                else if (User.IsInRole("Admin"))
                {
                    _logger.LogInformation("Admin retrieving all holds for showtime {ShowtimeId}.", showtimeId);
                }

                var result = holds.Select(h => new
                {
                    h.Id,
                    h.SeatId,
                    h.UserId,
                    h.ExpiresAt,
                    ExpiresIn = (h.ExpiresAt - DateTime.UtcNow).TotalSeconds,
                    IsExpired = h.ExpiresAt <= DateTime.UtcNow
                }).ToList();

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid argument: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active holds for showtime {ShowtimeId}.", showtimeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving holds." });
            }
        }

        /// <summary>
        /// Get all active seat holds for the current user.
        /// </summary>
        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUserHolds()
        {
            try
            {
                // ✅ Extract UserId from JWT token securely
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated." });

                _logger.LogInformation("Retrieving holds for user {UserId}.", userId);

                var holds = await _seatHoldService.GetHoldsByUserAsync(userId);

                var result = holds.Select(h => new
                {
                    h.Id,
                    h.ShowtimeId,
                    h.SeatId,
                    h.ExpiresAt,
                    ExpiresIn = (h.ExpiresAt - DateTime.UtcNow).TotalSeconds,
                    IsExpired = h.ExpiresAt <= DateTime.UtcNow
                }).ToList();

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid argument: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user holds.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving your holds." });
            }
        }

        /// <summary>
        /// Remove all expired seat holds from the system.
        /// Only admins can perform this operation.
        /// </summary>
        [HttpDelete("removeExpired")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveExpired()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("Admin {UserId} removing expired seat holds.", userId);

                await _seatHoldService.RemoveExpiredHoldsAsync();

                _logger.LogInformation("Expired seat holds removed by admin {UserId}.", userId);

                return Ok(new { message = "Expired holds have been removed successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing expired holds.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while removing expired holds." });
            }
        }
    }
}