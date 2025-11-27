using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
using System.Text;

namespace Movie_Reservation_System
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load additional configuration files
            builder.Configuration
                .AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);

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

            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
                options.TokenLifespan = TimeSpan.FromMinutes(180));

            #region Add JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("JWT");
            var authKey = jwtSettings["AuthKey"];

            if (!string.IsNullOrEmpty(authKey))
            {
                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                });
            }
            #endregion

            // Add controllers and swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            #region Dependency Injection
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
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();

            #endregion

            builder.Services.Configure<AdminConfiguration>(builder.Configuration.GetSection("AdminConfiguration"));
            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

            var app = builder.Build();

            // Seed admin user
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