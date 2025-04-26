using System;
using System.ComponentModel.DataAnnotations;

namespace AgriGreen.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Category { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Production Date")]
        public DateTime ProductionDate { get; set; }

        // Foreign key to Farmer
        [Required]
        public int FarmerId { get; set; }

        // Navigation property
        public Farmer Farmer { get; set; }
    }
}
