using Microsoft.EntityFrameworkCore;
using MovieReservationSystem.Core.Entities;

namespace MovieReservationSystem.Repository.Data.Configurations
{
    public class ReservationConfigurations : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Reservation> builder)
        {
            builder.HasOne(r => r.Seat)
                .WithMany(s => s.Reservations)
                .HasForeignKey(r => r.SeatId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Showtime)
                .WithMany()
                .HasForeignKey(r => r.ShowtimeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
