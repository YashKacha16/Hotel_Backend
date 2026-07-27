using System;
using System.ComponentModel.DataAnnotations;

namespace Hotel_Backend.Models
{
    public class RestaurantTable
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public int? CategoryId { get; set; }
        public TableCategory? Category { get; set; }

        public TableStatus Status { get; set; } = TableStatus.Free;

        public int Position { get; set; }

        public int? MergeGroupId { get; set; }
        public TableMergeGroup? MergeGroup { get; set; }

        [Required]
        public string QrToken { get; set; } = string.Empty;

        public DateTime LastStatusChangedAt { get; set; } = DateTime.UtcNow;

        public string? LastStatusChangedBy { get; set; }
    }
}
