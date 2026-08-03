using System;
using System.ComponentModel.DataAnnotations;

namespace Hotel_Backend.Models
{
    public class Chef
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Role { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
