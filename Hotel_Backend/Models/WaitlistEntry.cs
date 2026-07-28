using System;
using System.ComponentModel.DataAnnotations;

namespace Hotel_Backend.Models
{
    public class WaitlistEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Token { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string GuestName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        public int PartySize { get; set; }

        [StringLength(50)]
        public string? SeatingPreference { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public WaitlistStatus Status { get; set; } = WaitlistStatus.Waiting;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? AssignedTableId { get; set; }
        public RestaurantTable? AssignedTable { get; set; }
    }
}
