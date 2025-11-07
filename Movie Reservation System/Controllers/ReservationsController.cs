using Microsoft.AspNetCore.Mvc;
using Movie_Reservation_System.DTOs.Reservation;
using MovieReservationSystem.Core.Interfaces;

namespace Movie_Reservation_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.UserId))
                return BadRequest("Invalid reservation data.");

            try
            {
                var reservation = await _reservationService.CreateReservationAsync(dto.UserId, dto.ShowtimeId, dto.SeatId);
                return CreatedAtAction(nameof(GetReservationById), new { id = reservation!.Id }, reservation);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservationById(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
                return NotFound();

            return Ok(reservation);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserReservations(string userId)
        {
            var reservations = await _reservationService.GetUserReservationsAsync(userId);
            return Ok(reservations);
        }

        [HttpGet("showtime/{showtimeId}")]
        public async Task<IActionResult> GetShowtimeReservations(int showtimeId)
        {
            var reservations = await _reservationService.GetShowtimeReservationsAsync(showtimeId);
            return Ok(reservations);
        }

        [HttpPut("{id}/confirm")]
        public async Task<IActionResult> ConfirmReservation(int id)
        {
            try
            {
                await _reservationService.ConfirmReservationAsync(id);
                return Ok(new { message = "Reservation confirmed successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            try
            {
                await _reservationService.CancelReservationAsync(id);
                return Ok(new { message = "Reservation canceled successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
