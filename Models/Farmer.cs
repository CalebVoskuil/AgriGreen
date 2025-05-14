using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgriGreen.Models
{
    // Represents a farm business entity in the system
    // Acts as a profile for users with the "Farmer" role
    public class Farmer
    {
        // Unique identifier for the farmer entity
        public int Id { get; set; }

        // Name of the farm business
        // Used for display and identification purposes
        [Required, StringLength(100)]
        public string Name { get; set; }

        // Link to the AspNetUsers table one-to-one:
        // This establishes a connection between the domain model and ASP.NET Identity
        public string UserId { get; set; }
        
        // Navigation property to access the associated Identity user
        // Enables seamless integration with ASP.NET Core Identity system
        public IdentityUser User { get; set; }

        // Collection of products owned by this farmer
        // Enables navigation from farmer to their products
        public ICollection<Product> Products { get; set; }
    }
}
