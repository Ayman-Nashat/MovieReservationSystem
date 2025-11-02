namespace Movie_Reservation_System.DTOs.ShowTime
{
    public class ShowtimeDetailsDto
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;

        public int TheaterId { get; set; }
        public string TheaterName { get; set; } = string.Empty;

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TicketPrice { get; set; }
    }
}
