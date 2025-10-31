namespace Movie_Reservation_System.DTOs.Theater
{
    public class TheaterReadDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public string TheaterType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int? Rows { get; set; }
        public int? Columns { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
