using System.ComponentModel.DataAnnotations;

namespace Movie_Reservation_System.DTOs.Theater
{
    public class TheaterCreateDTO
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "TotalSeats must be greater than 0.")]
        public int TotalSeats { get; set; }

        [Range(1, 100, ErrorMessage = "Rows must be between 1 and 100.")]
        public int? Rows { get; set; }

        [Range(1, 100, ErrorMessage = "Columns must be between 1 and 100.")]
        public int? Columns { get; set; }

        [StringLength(50)]
        public string TheaterType { get; set; } = "Standard";

        public bool IsActive { get; set; } = true;
    }
}
