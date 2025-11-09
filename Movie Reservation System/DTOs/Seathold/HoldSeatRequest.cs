namespace Movie_Reservation_System.DTOs.Seathold
{
    public class HoldSeatRequest
    {
        public int ShowtimeId { get; set; }
        public int? SeatId { get; set; }

        // Optional fallback: frontend can send row/col instead of SeatId
        public int? RowNumber { get; set; }
        public int? ColumnNumber { get; set; }
        public int HoldMinutes { get; set; } = 5;
    }
}
