using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AgriGreen.Models
{
    // Represents a staff member with administrative access to the system
    // Users with this role can manage farmers and view all products
    public class Employee
    {
        // Unique identifier for the employee
        public int Id { get; set; }

        // Full name of the employee
        // Used for display and identification purposes
        [Required, StringLength(100)]
        public string Name { get; set; }

        // Optional link to the AspNetUsers table:
        // Connects this domain entity to the ASP.NET Core Identity system
        public string UserId { get; set; }
        
        // Navigation property to access the identity information
        // Enables access to authentication details for the employee
        public IdentityUser User { get; set; }
    }
}
