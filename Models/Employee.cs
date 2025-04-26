using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AgriGreen.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        // optional link to the AspNetUsers table:
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
    }
}
