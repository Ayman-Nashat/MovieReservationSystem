namespace MovieReservationSystem.Core.Entities
{
    public class Showtime
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // Foreign Keys
        public int MovieId { get; set; }
        public int TheaterId { get; set; }

        // Navigation Properties
        public Movie Movie { get; set; }
        public Theater Theater { get; set; }
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
