using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgriGreen.Models
{
    public class Farmer
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        // if you want to link to the AspNetUsers table one-to-one:
        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
