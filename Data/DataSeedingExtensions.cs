using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AgriGreen.Data
{
    public static class DataSeedingExtensions
    {
        /// <summary>
        /// Seeds the database with sample data if specified and if in development mode
        /// </summary>
        public static async Task<IHost> SeedDatabaseAsync(this IHost host, bool seedSampleData = false)
        {
            if (seedSampleData)
            {
                using (var scope = host.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var env = services.GetRequiredService<IHostEnvironment>();
                    
                    // Only seed in development
                    if (env.IsDevelopment())
                    {
                        var logger = services.GetRequiredService<ILogger<DataSeeder>>();
                        try
                        {
                            logger.LogInformation("Starting database seeding...");
                            
                            var context = services.GetRequiredService<ApplicationDbContext>();
                            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
                            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                            
                            var seeder = new DataSeeder(context, userManager, roleManager);
                            await seeder.SeedDataAsync();
                            
                            logger.LogInformation("Database seeding completed successfully.");
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "An error occurred during database seeding.");
                        }
                    }
                }
            }
            
            return host;
        }
    }
} 