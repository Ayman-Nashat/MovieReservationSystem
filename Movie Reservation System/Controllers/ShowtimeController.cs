using Microsoft.AspNetCore.Mvc;
using Movie_Reservation_System.DTOs.ShowTime;
using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Interfaces;
using MovieReservationSystem.Core.Service.Contract;

namespace Movie_Reservation_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShowtimeController : ControllerBase
    {
        private readonly IShowtimeService _showtimeService;
        private readonly ITheaterService _theaterService;
        private readonly IReservationService _reservationService;
        private readonly ISeatHoldService _seatHoldService;
        private readonly ISeatService _seatService;

        public ShowtimeController(IShowtimeService showtimeService, ITheaterService theaterService, ISeatHoldService seatHoldService, IReservationService reservationService, ISeatService seatService)
        {
            _showtimeService = showtimeService;
            _theaterService = theaterService;
            _seatHoldService = seatHoldService;
            _reservationService = reservationService;
            _seatService = seatService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var showtimes = await _showtimeService.GetAllShowtimesAsync();

            var result = showtimes.Select(sh => new ShowtimeDetailsDto
            {
                Id = sh.Id,
                MovieId = sh.MovieId,
                MovieTitle = sh.Movie?.Title ?? string.Empty,
                TheaterId = sh.TheaterId,
                TheaterName = sh.Theater?.Name ?? string.Empty,
                StartTime = sh.StartTime,
                EndTime = sh.EndTime,
                TicketPrice = sh.TicketPrice
            });


            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var showtime = await _showtimeService.GetShowtimeByIdAsync(id);
            if (showtime == null) return NotFound();
            var res = new ShowtimeDetailsDto
            {
                Id = showtime.Id,
                MovieId = showtime.MovieId,
                MovieTitle = showtime.Movie.Title,
                TheaterId = showtime.TheaterId,
                TheaterName = showtime.Theater.Name,
                StartTime = showtime.StartTime,
                EndTime = showtime.EndTime,
                TicketPrice = showtime.TicketPrice
            };

            return Ok(res);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddShowtimeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.EndTime <= dto.StartTime) return BadRequest("EndTime must be after StartTime.");

            var entity = new Showtime
            {
                MovieId = dto.MovieId,
                TheaterId = dto.TheaterId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                TicketPrice = dto.TicketPrice
            };

            try
            {
                var created = await _showtimeService.AddShowtimeAsync(entity);
                var createdWithDetails = await _showtimeService.GetShowtimeByIdAsync(created.Id);
                if (createdWithDetails == null)
                    return BadRequest("Failed to retrieve created showtime.");

                var resultDto = new ShowtimeDetailsDto
                {
                    Id = createdWithDetails.Id,
                    MovieId = createdWithDetails.MovieId,
                    MovieTitle = createdWithDetails.Movie?.Title ?? "",
                    TheaterId = createdWithDetails.TheaterId,
                    TheaterName = createdWithDetails.Theater?.Name ?? "",
                    StartTime = createdWithDetails.StartTime,
                    EndTime = createdWithDetails.EndTime,
                    TicketPrice = createdWithDetails.TicketPrice
                };

                return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateShowtimeDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dto.Id != id) return BadRequest("Id in route and body must match.");
            if (dto.EndTime <= dto.StartTime)
                return BadRequest("EndTime must be after StartTime.");

            var entity = new Showtime
            {
                Id = dto.Id,
                MovieId = dto.MovieId,
                TheaterId = dto.TheaterId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                TicketPrice = dto.TicketPrice
            };

            try
            {
                var updated = await _showtimeService.UpdateShowtimeAsync(entity);
                if (updated == null) return NotFound();

                return Ok(new { id = updated.Id, dto });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _showtimeService.DeleteShowtimeAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
        [HttpGet("{showtimeId}/seats")]
        public async Task<IActionResult> GetSeatMap(int showtimeId)
        {
            var showtime = await _showtimeService.GetShowtimeByIdAsync(showtimeId);
            if (showtime == null)
                return NotFound("Showtime not found.");

            var theaterId = showtime.TheaterId;

            // Get all seats for this theater
            var allSeats = await _seatService.GetSeatsByTheaterIdAsync(theaterId);

            // Get reservations for this showtime
            var reservedSeatIds = (await _reservationService.GetShowtimeReservationsAsync(showtimeId))
                .Select(r => r.SeatId)
                .ToHashSet();

            // Get active holds for this showtime
            var activeHolds = await _seatHoldService.GetActiveHoldsAsync(showtimeId, allSeats.Select(s => s.Id));
            var heldSeatIds = activeHolds.Select(h => h.SeatId).ToHashSet();

            // Build result
            var seatMap = allSeats.Select(seat => new
            {
                seat.Id,
                seat.RowNumber,
                seat.ColumnNumber,
                seat.SeatNumber,
                seat.SeatType,
                Status = reservedSeatIds.Contains(seat.Id)
                    ? "reserved"
                    : heldSeatIds.Contains(seat.Id)
                        ? "held"
                        : "available"
            });

            return Ok(seatMap);
        }


    }
}
