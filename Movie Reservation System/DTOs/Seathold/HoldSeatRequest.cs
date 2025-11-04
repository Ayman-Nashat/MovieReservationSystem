namespace Movie_Reservation_System.DTOs.Seathold
{
    public class HoldSeatRequest
    {
        public int ShowtimeId { get; set; }
        public int SeatId { get; set; }
        public int HoldMinutes { get; set; } = 5;
    }
}
