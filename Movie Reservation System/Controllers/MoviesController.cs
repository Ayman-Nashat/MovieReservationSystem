using Microsoft.AspNetCore.Mvc;
using Movie_Reservation_System.DTOs.Movie;
using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Service.Contract;

namespace Movie_Reservation_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;
        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var movies = await _movieService.GetAllMoviesAsync();
            return Ok(movies);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null)
                return NotFound($"Movie with ID {id} not found.");

            return Ok(movie);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddMovieDTO movie)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            Movie movie1 = new Movie
            {
                Title = movie.Title,
                Genre = movie.Genre,
                Description = movie.Description,
                DurationMinutes = movie.DurationMinutes
            };

            await _movieService.AddMovieAsync(movie1);
            return CreatedAtAction(nameof(GetById), new { id = movie1.Id }, movie1);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Movie movie)
        {
            if (id != movie.Id)
                return BadRequest("ID in URL and body do not match.");

            var existing = await _movieService.GetMovieByIdAsync(id);
            if (existing == null)
                return NotFound($"Movie with ID {id} not found.");

            existing.Title = movie.Title;
            existing.Genre = movie.Genre;

            await _movieService.AddMovieAsync(existing);
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null)
                return NotFound($"Movie with ID {id} not found.");

            await _movieService.DeleteMovieAsync(id);
            return NoContent();
        }
    }
}
