namespace Movie_Reservation_System.DTOs.Movie
{
    public class UpdateMovieDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
    }
}
