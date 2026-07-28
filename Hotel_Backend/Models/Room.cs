using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_Backend.Models
{
    public class Room
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Number { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        
        [ForeignKey("CategoryId")]
        public RoomCategory? Category { get; set; }

        [MaxLength(20)]
        public string? Floor { get; set; }

        public int Capacity { get; set; } = 2;

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Available"; // Available, Occupied, Maintenance, Out of service

        public List<string> Amenities { get; set; } = new();

        public List<string> Images { get; set; } = new();

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
