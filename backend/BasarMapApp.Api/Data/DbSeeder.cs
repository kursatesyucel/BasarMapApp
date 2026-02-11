using BasarMapApp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BasarMapApp.Api.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAdminUser(ApplicationDbContext context)
        {
            // Check if admin user already exists
            var adminExists = await context.Users.AnyAsync(u => u.Username == "admin");
            
            if (!adminExists)
            {
                var adminUser = new User
                {
                    Username = "admin",
                    // Password: Admin123!
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
                
                Console.WriteLine("✓ Admin user seeded successfully (Username: admin, Password: Admin123!)");
            }
        }
    }
}
