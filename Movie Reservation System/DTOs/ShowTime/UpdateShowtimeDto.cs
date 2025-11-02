using System.ComponentModel.DataAnnotations;

namespace Movie_Reservation_System.DTOs.ShowTime
{
    public class UpdateShowtimeDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int MovieId { get; set; }

        [Required]
        public int TheaterId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TicketPrice { get; set; }
    }
}
