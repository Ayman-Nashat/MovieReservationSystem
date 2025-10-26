namespace Movie_Reservation_System.DTOs.Reservation
{
    public class CreateReservationDto
    {
        public string UserId { get; set; } = string.Empty;
        public int ShowtimeId { get; set; }
        public int SeatId { get; set; }
    }
}
