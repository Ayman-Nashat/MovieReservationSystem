using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MovieReservationSystem.Core.Entities;

namespace MovieReservationSystem.Repository.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Theater> Theaters { get; set; }
        public DbSet<Showtime> Showtimes { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Seat -> Theater (do not cascade)
            builder.Entity<Seat>()
                .HasOne(s => s.Theater)
                .WithMany(t => t.Seats)
                .HasForeignKey(s => s.TheaterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Showtime -> Theater (do not cascade)
            builder.Entity<Showtime>()
                .HasOne(s => s.Theater)
                .WithMany(t => t.Showtimes)
                .HasForeignKey(s => s.TheaterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reservation -> Seat
            builder.Entity<Reservation>()
                .HasOne(r => r.Seat)
                .WithMany(s => s.Reservations)
                .HasForeignKey(r => r.SeatId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reservation -> Showtime
            builder.Entity<Reservation>()
                .HasOne(r => r.Showtime)
                .WithMany(s => s.Reservations)
                .HasForeignKey(r => r.ShowtimeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reservation -> User (Identity user)
            builder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict); base.OnModelCreating(builder);

            builder.Entity<Seat>()
                .HasIndex(s => new { s.TheaterId, s.SeatNumber })
                .IsUnique();

            builder.Entity<Reservation>()
                .HasIndex(r => new { r.ShowtimeId, r.SeatId })
                .IsUnique();

            builder.Entity<Showtime>()
                .HasIndex(s => new { s.TheaterId, s.StartTime }).IsUnique(false);

        }
    }
}
