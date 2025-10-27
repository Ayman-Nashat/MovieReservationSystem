using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Repository.Data.Configurations;


namespace MovieReservationSystem.Repository.Data
{
    public static class AppDbSeeder
    {
        public static async Task SeedAdminAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            // Ensure roles exist
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            // Get AdminSettings from appsettings.json
            var adminSettings = configuration.GetSection("AdminConfiguration").Get<AdminConfiguration>();

            if (await userManager.FindByEmailAsync(adminSettings.Email) == null)
            {
                var admin = new User
                {
                    UserName = "admin",
                    Email = adminSettings.Email,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(admin, adminSettings.Password);
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
