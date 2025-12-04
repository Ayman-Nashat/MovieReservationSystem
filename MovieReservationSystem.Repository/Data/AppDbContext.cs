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
        public DbSet<Genre> Genres { get; set; }
        public DbSet<MovieGenre> MovieGenres { get; set; }
        public DbSet<SeatHold> SeatHolds { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Seat>()
                .HasOne(s => s.Theater)
                .WithMany(t => t.Seats)
                .HasForeignKey(s => s.TheaterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Showtime>()
                .HasOne(s => s.Theater)
                .WithMany(t => t.Showtimes)
                .HasForeignKey(s => s.TheaterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Reservation>()
                .HasOne(r => r.Seat)
                .WithMany(s => s.Reservations)
                .HasForeignKey(r => r.SeatId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Reservation>()
                .HasOne(r => r.Showtime)
                .WithMany(s => s.Reservations)
                .HasForeignKey(r => r.ShowtimeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Reservation>()
                .HasIndex(r => new { r.ShowtimeId, r.SeatId })
                .IsUnique();

            builder.Entity<Seat>()
                .HasIndex(s => new { s.TheaterId, s.SeatNumber })
                .IsUnique();

            builder.Entity<Showtime>()
                .HasIndex(s => new { s.TheaterId, s.StartTime }).IsUnique(false);

            builder.Entity<MovieGenre>()
                .HasKey(mg => new { mg.MovieId, mg.GenreId });

            builder.Entity<SeatHold>()
                .HasIndex(h => new { h.ShowtimeId, h.SeatId });

            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            builder.Entity<Theater>()
               .HasIndex(t => new { t.Name, t.Location })
               .IsUnique();

            builder.Entity<Showtime>()
                .Property(s => s.TicketPrice)
                .HasPrecision(18, 2);

            builder.Entity<Payment>()
                .HasIndex(p => p.PaymentIntentId)
                .IsUnique();

            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);
        }
    }
}
