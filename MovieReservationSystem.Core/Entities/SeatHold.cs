namespace MovieReservationSystem.Core.Entities
{
    public class SeatHold
    {
        public int Id { get; set; }

        public int ShowtimeId { get; set; }
        public Showtime Showtime { get; set; } = null!;

        public int SeatId { get; set; }
        public Seat Seat { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
