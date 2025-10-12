namespace MovieReservationSystem.Core.Entities
{
    public class Seat
    {
        public int Id { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public string SeatType { get; set; } = string.Empty;

        // Foreign Key
        public int TheaterId { get; set; }

        // Navigation Property
        public Theater Theater { get; set; }
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
