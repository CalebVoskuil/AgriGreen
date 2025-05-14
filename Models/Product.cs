using System;
using System.ComponentModel.DataAnnotations;

namespace AgriGreen.Models
{
    // Represents an agricultural product produced by a farmer
    // Core entity that forms the inventory management system
    public class Product
    {
        // Unique identifier for the product
        [Key]
        public int Id { get; set; }

        // Name of the product (e.g., "Organic Tomatoes")
        // Required with maximum length constraint for database optimization
        [Required, StringLength(100)]
        public string Name { get; set; }

        // Product category (e.g., "Vegetables", "Fruits", "Dairy")
        // Used for product classification and filtering
        [Required, StringLength(50)]
        public string Category { get; set; }

        // Date when the product was produced/harvested
        // Important for inventory tracking and freshness monitoring
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Production Date")]
        public DateTime ProductionDate { get; set; }

        // Foreign key to Farmer
        // Links each product to its producer/owner
        [Required]
        public int FarmerId { get; set; }

        // Navigation property - made nullable to avoid validation issues
        // Enables entity framework to load related farmer data efficiently
        public Farmer? Farmer { get; set; }
    }
}
