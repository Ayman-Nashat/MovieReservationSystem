using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Movie_Reservation_System.Helper;
using Movie_Reservation_System.Settings;
using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Interfaces;
using MovieReservationSystem.Core.Repositort.Contract;
using MovieReservationSystem.Core.Repository.Contract;
using MovieReservationSystem.Core.Service.Contract;
using MovieReservationSystem.Core.Services;
using MovieReservationSystem.Repository.Data;
using MovieReservationSystem.Repository.Data.Configurations;
using MovieReservationSystem.Repository.Repositories;
using MovieReservationSystem.Service;

namespace Movie_Reservation_System
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add Identity
            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromMinutes(180));


            // Add controllers and swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            #region Depandancy Injection
            builder.Services.AddScoped<IMovieRepository, MovieRepository>();
            builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
            builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
            builder.Services.AddScoped<IReservationService, ReservationService>();
            builder.Services.AddScoped<ISeatHoldRepository, SeatHoldRepository>();
            builder.Services.AddScoped<ISeatHoldService, SeatHoldService>();
            builder.Services.AddScoped<IMovieService, MovieService>();
            builder.Services.AddScoped<ITheaterService, TheaterService>();
            builder.Services.AddScoped<ITheaterRepository, TheaterRepository>();
            builder.Services.AddScoped<IGenreService, GenreService>();
            builder.Services.AddScoped<ISeatService, SeatService>();
            builder.Services.AddScoped<ISeatRepository, SeatRepository>();
            builder.Services.AddTransient<IMailService, EmailSettings>();
            builder.Services.AddScoped<IShowtimeRepository, ShowtimeRepository>();
            builder.Services.AddScoped<IShowtimeService, ShowtimeService>();

            #endregion

            builder.Services.Configure<AdminConfiguration>(builder.Configuration.GetSection("AdminConfiguration"));
            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

            // Load additional local config (if file exists)
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var userManager = services.GetRequiredService<UserManager<User>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var configuration = services.GetRequiredService<IConfiguration>();

                await AppDbSeeder.SeedAdminAsync(userManager, roleManager, configuration);
            }

            // Middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}