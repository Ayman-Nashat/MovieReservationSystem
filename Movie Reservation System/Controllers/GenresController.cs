using Microsoft.AspNetCore.Mvc;
using Movie_Reservation_System.DTOs.Genre;
using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Service.Contract;

namespace Movie_Reservation_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly IGenreService _genreService;
        private readonly IMovieService _movieService;


        public GenresController(IGenreService genreService, IMovieService movieService)
        {
            _genreService = genreService;
            _movieService = movieService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var genres = await _genreService.GetAllAsync();
            return Ok(genres);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var genre = await _genreService.GetByIdAsync(id);
            if (genre == null)
                return NotFound();

            return Ok(genre);
        }
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddGenreDTO genre)
        {
            Genre genre1 = new Genre
            {
                Name = genre.Name
            };
            await _genreService.AddAsync(genre1);
            await _genreService.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = genre1.Id }, genre);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AddGenreDTO genreDto)
        {
            var existingGenre = await _genreService.GetByIdAsync(id);
            if (existingGenre == null)
                return NotFound();

            existingGenre.Name = genreDto.Name;

            _genreService.Update(existingGenre);
            await _genreService.SaveChangesAsync();

            return Ok(existingGenre);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var genre = await _genreService.GetByIdAsync(id);
            if (genre == null)
                return NotFound();

            _genreService.Delete(genre);
            await _genreService.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{genreId}/add-to-movie/{movieId}")]
        public async Task<IActionResult> AddGenreToMovie(int genreId, int movieId)
        {
            var movie = await _movieService.GetMovieByIdAsync(movieId);
            if (movie == null)
                return NotFound($"Movie with ID {movieId} not found.");

            var genre = await _genreService.GetByIdAsync(genreId);
            if (genre == null)
                return NotFound($"Genre with ID {genreId} not found.");

            // check if genre already linked to movie
            bool alreadyLinked = movie.MovieGenres?.Any(mg => mg.GenreId == genreId) ?? false;
            if (alreadyLinked)
                return BadRequest("This genre is already linked to the selected movie.");

            // link them
            await _genreService.AddGenreToMovieAsync(genreId, movieId);

            return Ok($"Genre '{genre.Name}' added to movie '{movie.Title}'.");
        }



    }
}
