using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_Backend.Models
{
    public class SeasonalRule
    {
        [Key]
        public int Id { get; set; }

        public int RoomCategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// If true, this rule recurs (e.g. every weekend). Use DaysOfWeek to specify which days.
        /// </summary>
        public bool IsRecurring { get; set; } = false;

        /// <summary>
        /// Comma-separated day names, e.g. "Saturday,Sunday"
        /// </summary>
        [MaxLength(100)]
        public string? DaysOfWeek { get; set; }

        /// <summary>
        /// Price modifier as a percentage. E.g. +30 means 30% surcharge, -10 means 10% discount.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceModifierPercent { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("RoomCategoryId")]
        public RoomCategory RoomCategory { get; set; } = null!;
    }
}
