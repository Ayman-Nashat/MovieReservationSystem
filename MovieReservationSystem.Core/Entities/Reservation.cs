namespace MovieReservationSystem.Core.Entities
{
    public class Reservation
    {
        public int Id { get; set; }
        public DateTime ReservationDate { get; set; }
        public string Status { get; set; } = "Pending";

        // Foreign Keys
        public string UserId { get; set; }
        public int ShowtimeId { get; set; }
        public int SeatId { get; set; }

        // Navigation Properties
        public User User { get; set; }
        public Showtime Showtime { get; set; }
        public Seat Seat { get; set; }
        //public Payment Payment { get; set; }
    }
}
