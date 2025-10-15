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

            var movieDtos = movies.Select(m => new MovieDTO
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                DurationMinutes = m.DurationMinutes,
                Genres = m.MovieGenres.Select(mg => mg.Genre.Name).ToList()
            });

            return Ok(movieDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null)
                return NotFound();

            var movieDto = new MovieDTO
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                DurationMinutes = movie.DurationMinutes,
                Genres = movie.MovieGenres.Select(mg => mg.Genre.Name).ToList()
            };

            return Ok(movieDto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddMovieDTO movie)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            Movie movie1 = new Movie
            {
                Title = movie.Title,
                Description = movie.Description,
                DurationMinutes = movie.DurationMinutes
            };

            await _movieService.AddMovieAsync(movie1);
            return CreatedAtAction(nameof(GetById), new { id = movie1.Id }, movie1);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMovieDTO movie)
        {
            if (id != movie.Id)
                return BadRequest("ID in URL and body do not match.");

            var existing = await _movieService.GetMovieByIdAsync(id);
            if (existing == null)
                return NotFound($"Movie with ID {id} not found.");

            existing.Title = movie.Title;

            Movie updatedMovie = new Movie
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                DurationMinutes = movie.DurationMinutes
            };

            await _movieService.AddMovieAsync(updatedMovie);
            return Ok(updatedMovie);
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
