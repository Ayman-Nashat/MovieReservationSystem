using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movie_Reservation_System.DTOs.Reservation;
using MovieReservationSystem.Core.Interfaces;
using System.Security.Claims;

namespace Movie_Reservation_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(
            IReservationService reservationService,
            ILogger<ReservationsController> logger)
        {
            _reservationService = reservationService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new reservation for a seat at a showtime.
        /// Automatically releases any existing hold on this seat by the user.
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid reservation data." });

            // ⚠️ SECURITY: Validate that requested UserId matches authenticated user
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { message = "User not authenticated." });

            if (dto.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                _logger.LogWarning("Unauthorized reservation attempt: User {CurrentUserId} tried to create reservation for user {RequestedUserId}.",
                    currentUserId, dto.UserId);
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only create reservations for yourself." });
            }

            // Validate DTO
            if (string.IsNullOrWhiteSpace(dto.UserId))
                return BadRequest(new { message = "User ID is required." });
            if (dto.ShowtimeId <= 0)
                return BadRequest(new { message = "Showtime ID must be greater than 0." });
            if (dto.SeatId <= 0)
                return BadRequest(new { message = "Seat ID must be greater than 0." });

            try
            {
                _logger.LogInformation("Creating reservation for user {UserId}, showtime {ShowtimeId}, seat {SeatId}.",
                    dto.UserId, dto.ShowtimeId, dto.SeatId);

                var reservation = await _reservationService.CreateReservationAsync(
                    dto.UserId,
                    dto.ShowtimeId,
                    dto.SeatId);

                return CreatedAtAction(nameof(GetReservationById), new { id = reservation!.Id }, new
                {
                    reservation.Id,
                    reservation.UserId,
                    reservation.ShowtimeId,
                    reservation.SeatId,
                    reservation.Status,
                    reservation.ReservationDate
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Conflict creating reservation: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid argument: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating reservation.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An unexpected error occurred while creating the reservation." });
            }
        }

        /// <summary>
        /// Get a specific reservation by ID.
        /// Users can only view their own reservations unless they are admins.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetReservationById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid reservation ID." });

                var reservation = await _reservationService.GetReservationByIdAsync(id);
                if (reservation == null)
                    return NotFound(new { message = "Reservation not found." });

                // ✅ Authorization check: Only owner or admin can view
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId != null && userId != reservation.UserId && !User.IsInRole("Admin"))
                {
                    _logger.LogWarning("Unauthorized access: User {UserId} tried to view reservation {ReservationId}.",
                        userId, id);
                    return Forbid();
                }

                return Ok(new
                {
                    reservation.Id,
                    reservation.UserId,
                    reservation.ShowtimeId,
                    reservation.SeatId,
                    reservation.Status,
                    reservation.ReservationDate,
                    Seat = new { reservation.Seat?.SeatNumber },
                    Showtime = new { reservation.Showtime?.StartTime },
                    Movie = new { reservation.Showtime?.Movie?.Title }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation {ReservationId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the reservation." });
            }
        }

        /// <summary>
        /// Get all reservations for a specific user.
        /// Users can only view their own reservations unless they are admins.
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserReservations(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return BadRequest(new { message = "Invalid user ID." });

                // ✅ Authorization check: Only owner or admin can view
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized(new { message = "User not authenticated." });

                if (currentUserId != userId && !User.IsInRole("Admin"))
                {
                    _logger.LogWarning("Unauthorized access: User {CurrentUserId} tried to view user {UserId} reservations.",
                        currentUserId, userId);
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only view your own reservations." });
                }

                var reservations = await _reservationService.GetUserReservationsAsync(userId);

                return Ok(reservations.Select(r => new
                {
                    r.Id,
                    r.UserId,
                    r.ShowtimeId,
                    r.SeatId,
                    r.Status,
                    r.ReservationDate,
                    Seat = new { r.Seat?.SeatNumber },
                    Showtime = new { r.Showtime?.StartTime },
                    Movie = new { r.Showtime?.Movie?.Title }
                }));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservations for user {UserId}.", userId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving reservations." });
            }
        }

        /// <summary>
        /// Get all reservations for a specific showtime.
        /// Only admins can view this information.
        /// </summary>
        [HttpGet("showtime/{showtimeId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetShowtimeReservations(int showtimeId)
        {
            try
            {
                if (showtimeId <= 0)
                    return BadRequest(new { message = "Invalid showtime ID." });

                var reservations = await _reservationService.GetShowtimeReservationsAsync(showtimeId);

                return Ok(reservations.Select(r => new
                {
                    r.Id,
                    r.UserId,
                    r.SeatId,
                    r.Status,
                    r.ReservationDate,
                    Seat = new { r.Seat?.SeatNumber },
                    User = new { r.User?.Email }
                }));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservations for showtime {ShowtimeId}.", showtimeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving reservations." });
            }
        }

        /// <summary>
        /// Confirm a pending reservation.
        /// Users can only confirm their own reservations unless they are admins.
        /// </summary>
        [HttpPut("{id}/confirm")]
        [Authorize]
        public async Task<IActionResult> ConfirmReservation(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid reservation ID." });

                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized(new { message = "User not authenticated." });

                var reservation = await _reservationService.GetReservationByIdAsync(id);

                if (reservation == null)
                    return NotFound(new { message = "Reservation not found." });

                // ✅ Authorization check: Only owner or admin can confirm
                if (currentUserId != reservation.UserId && !User.IsInRole("Admin"))
                {
                    _logger.LogWarning("Unauthorized confirmation: User {UserId} tried to confirm reservation {ReservationId}.",
                        currentUserId, id);
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only confirm your own reservations." });
                }

                await _reservationService.ConfirmReservationAsync(id);

                _logger.LogInformation("Reservation {ReservationId} confirmed by user {UserId}.", id, currentUserId);

                return Ok(new { message = "Reservation confirmed successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Reservation not found." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot confirm reservation: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming reservation {ReservationId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while confirming the reservation." });
            }
        }

        /// <summary>
        /// Cancel an existing reservation.
        /// Users can only cancel their own reservations unless they are admins.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> CancelReservation(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid reservation ID." });

                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized(new { message = "User not authenticated." });

                var reservation = await _reservationService.GetReservationByIdAsync(id);

                if (reservation == null)
                    return NotFound(new { message = "Reservation not found." });

                // ✅ Authorization check: Only owner or admin can cancel
                if (currentUserId != reservation.UserId && !User.IsInRole("Admin"))
                {
                    _logger.LogWarning("Unauthorized cancellation: User {UserId} tried to cancel reservation {ReservationId}.",
                        currentUserId, id);
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only cancel your own reservations." });
                }

                await _reservationService.CancelReservationAsync(id);

                _logger.LogInformation("Reservation {ReservationId} canceled by user {UserId}.", id, currentUserId);

                return NoContent(); // 204 No Content
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Reservation not found." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot cancel reservation: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling reservation {ReservationId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while canceling the reservation." });
            }
        }
    }
}