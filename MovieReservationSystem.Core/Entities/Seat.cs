namespace MovieReservationSystem.Core.Entities
{
    public class Seat : BaseEntity<int>
    {
        public string SeatNumber { get; set; } = string.Empty;
        public string SeatType { get; set; } = string.Empty;
        public int TheaterId { get; set; }
        public Theater Theater { get; set; } = null!;
        public int RowNumber { get; set; }
        public int ColumnNumber { get; set; }
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
