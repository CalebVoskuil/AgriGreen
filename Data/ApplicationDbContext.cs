using AgriGreen.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AgriGreen.Data
{
    // Database context that integrates ASP.NET Identity with application data
    // Central class for Entity Framework Core ORM configuration
    public class ApplicationDbContext : IdentityDbContext
    {
        // Constructor that accepts options for configuration
        // Options typically include connection string and database provider
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        // Collection of farm business entities in the system
        // Serves as the entry point for farmer data operations
        public DbSet<Farmer> Farmers { get; set; }
        
        // Collection of system administrators and staff
        // Used for employee data management
        public DbSet<Employee> Employees { get; set; }
        
        // Collection of agricultural products
        // Primary domain entity for inventory management
        public DbSet<Product> Products { get; set; }

    }
}
