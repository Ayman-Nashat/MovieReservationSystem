namespace Movie_Reservation_System.DTOs.Movie
{
    public class MovieDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int DurationMinutes { get; set; }
        public List<string> Genres { get; set; } = new();
    }
}
