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
using MovieReservationSystem.Repository.Repositories;
using MovieReservationSystem.Service;

namespace Movie_Reservation_System
{
    public class Program
    {
        public static void Main(string[] args)
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

            //Depandancy Injection
            builder.Services.AddScoped<IMovieRepository, MovieRepository>();
            builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
            builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
            builder.Services.AddScoped<ISeatHoldRepository, SeatHoldRepository>();
            builder.Services.AddScoped<IMovieService, MovieService>();
            builder.Services.AddScoped<IGenreService, GenreService>();
            builder.Services.AddScoped<IReservationService, ReservationService>();
            builder.Services.AddTransient<IMailService, EmailSettings>();

            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

            var app = builder.Build();

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