using Microsoft.AspNetCore.Mvc;
using Movie_Reservation_System.DTOs.Theater;
using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Service.Contract;

namespace Movie_Reservation_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TheaterController : ControllerBase
    {
        private readonly ITheaterService _theaterService;

        public TheaterController(ITheaterService theaterService)
        {
            _theaterService = theaterService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTheaters()
        {
            var theaters = await _theaterService.GetAllTheatersAsync();
            var theaterDtos = theaters.Select(t => new TheaterReadDTO
            {
                Id = t.Id,
                Name = t.Name,
                Location = t.Location,
                TotalSeats = t.TotalSeats,
                Rows = t.Rows,
                Columns = t.Columns,
                TheaterType = t.TheaterType,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            });
            return Ok(theaterDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTheaterById(int id)
        {
            var theater = await _theaterService.GetTheaterByIdAsync(id);
            if (theater == null)
                return NotFound($"Theater with ID {id} not found.");

            var dto = new TheaterReadDTO
            {
                Id = theater.Id,
                Name = theater.Name,
                Location = theater.Location,
                TotalSeats = theater.TotalSeats,
                Rows = theater.Rows,
                Columns = theater.Columns,
                TheaterType = theater.TheaterType,
                IsActive = theater.IsActive,
                CreatedAt = theater.CreatedAt
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTheater([FromBody] TheaterCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var theater = new Theater
            {
                Name = dto.Name,
                Location = dto.Location,
                TotalSeats = dto.TotalSeats,
                Rows = dto.Rows,
                Columns = dto.Columns,
                TheaterType = dto.TheaterType,
                IsActive = true
            };

            await _theaterService.AddTheaterAsync(theater);

            var responseDto = new TheaterReadDTO
            {
                Id = theater.Id,
                Name = theater.Name,
                Location = theater.Location,
                TotalSeats = theater.TotalSeats,
                Rows = theater.Rows,
                Columns = theater.Columns,
                TheaterType = theater.TheaterType,
                IsActive = theater.IsActive,
                CreatedAt = theater.CreatedAt
            };

            return CreatedAtAction(nameof(GetTheaterById), new { id = theater.Id }, responseDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTheater(int id, [FromBody] TheaterUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var theater = await _theaterService.GetTheaterByIdAsync(id);
            if (theater == null)
                return NotFound($"Theater with ID {id} not found.");

            theater.Name = dto.Name ?? theater.Name;
            theater.Location = dto.Location ?? theater.Location;
            theater.TotalSeats = dto.TotalSeats;
            theater.Rows = dto.Rows ?? theater.Rows;
            theater.Columns = dto.Columns ?? theater.Columns;
            theater.TheaterType = dto.TheaterType ?? theater.TheaterType;
            theater.IsActive = dto.IsActive;
            theater.UpdatedAt = DateTime.UtcNow;

            await _theaterService.UpdateTheaterAsync(theater);

            var updatedDto = new TheaterReadDTO
            {
                Id = theater.Id,
                Name = theater.Name,
                Location = theater.Location,
                TotalSeats = theater.TotalSeats,
                Rows = theater.Rows,
                Columns = theater.Columns,
                TheaterType = theater.TheaterType,
                IsActive = theater.IsActive,
                CreatedAt = theater.CreatedAt
            };

            return Ok(updatedDto);
        }

        // DELETE: api/Theater/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTheater(int id)
        {
            var theater = await _theaterService.GetTheaterByIdAsync(id);
            if (theater == null)
                return NotFound($"Theater with ID {id} not found.");

            await _theaterService.DeleteTheaterAsync(id);
            return NoContent();
        }
    }
}
