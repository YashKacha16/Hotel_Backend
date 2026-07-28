using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_Backend.Models
{
    public class RoomCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }

        [MaxLength(10)]
        public string Currency { get; set; } = "INR";

        public bool SeasonalPricingEnabled { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public int? Capacity { get; set; }

        [MaxLength(500)]
        public string? Amenities { get; set; }

        [MaxLength(1000)]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<SeasonalRule> SeasonalRules { get; set; } = new List<SeasonalRule>();
    }
}
