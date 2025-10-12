using Microsoft.AspNetCore.Identity;

namespace MovieReservationSystem.Core.Entities
{
    public class User : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    }
}
