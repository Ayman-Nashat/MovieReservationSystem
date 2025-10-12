namespace Movie_Reservation_System.DTOs.Movie
{
    public class AddMovieDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
    }
}
