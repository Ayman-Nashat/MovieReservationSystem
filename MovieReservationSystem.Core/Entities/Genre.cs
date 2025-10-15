namespace MovieReservationSystem.Core.Entities
{
    public class Genre : BaseEntity<int>
    {
        public string Name { get; set; } = string.Empty;

        public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    }
}
