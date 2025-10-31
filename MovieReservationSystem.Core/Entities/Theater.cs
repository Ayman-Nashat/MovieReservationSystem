using System.ComponentModel.DataAnnotations;

namespace MovieReservationSystem.Core.Entities
{
    public class Theater : BaseEntity<int>
    {

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        public int TotalSeats { get; set; }

        public int? Rows { get; set; }
        public int? Columns { get; set; }

        [StringLength(50)]
        public string TheaterType { get; set; } = "Standard";
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        public Guid ExternalId { get; set; } = Guid.NewGuid();

        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
        public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
    }
}
