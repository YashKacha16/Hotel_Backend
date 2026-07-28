using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_Backend.Models
{
    public class HotelSetting
    {
        [Key]
        public int Id { get; set; }

        public int? HotelId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? LogoUrl { get; set; }

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = "INR (₹)";

        [Range(0, 100)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ServiceChargePercent { get; set; } = 0;

        [Range(0, 100)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CgstPercent { get; set; } = 0;

        [Range(0, 100)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SgstPercent { get; set; } = 0;

        public int WaitlistEstimatedWaitMinutes { get; set; } = 22;

        [MaxLength(200)]
        public string WaitlistMessage { get; set; } = "Based on average turnover of 48m over the last hour and 3 free tables.";

        [Range(0, 100)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MinimumAdvancePercent { get; set; } = 0;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
