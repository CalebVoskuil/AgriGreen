using AgriGreen.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace AgriGreen.Tools
{
    public class SeedDatabase
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting database seeding process...");
            
            var host = CreateHostBuilder(args).Build();
            
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    
                    var seeder = new DataSeeder(context, userManager, roleManager);
                    await seeder.SeedDataAsync();
                    
                    Console.WriteLine("Database seeding completed successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred during database seeding: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Register DbContext
                    if (hostContext.HostingEnvironment.IsDevelopment())
                    {
                        services.AddDbContext<ApplicationDbContext>(options =>
                            options.UseSqlite(hostContext.Configuration.GetConnectionString("DefaultConnection")));
                    }
                    else
                    {
                        services.AddDbContext<ApplicationDbContext>(options =>
                            options.UseSqlServer(hostContext.Configuration.GetConnectionString("AzureConnection")));
                    }

                    // Register Identity services
                    services.AddIdentity<IdentityUser, IdentityRole>()
                        .AddEntityFrameworkStores<ApplicationDbContext>()
                        .AddDefaultTokenProviders();
                });
    }
} 