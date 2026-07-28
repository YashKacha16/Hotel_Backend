using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_Backend.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        public string BookingCode { get; set; } = string.Empty;

        public string GuestName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public string IdProofUrl { get; set; } = string.Empty;

        public int RoomId { get; set; }
        [ForeignKey("RoomId")]
        public Room? Room { get; set; }

        public DateTime CheckInDate { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public DateTime CheckOutDate { get; set; }

        public string Source { get; set; } = "Walk-in";
        public int Guests { get; set; }
        public decimal AdvanceAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;

        // Status: Confirmed, Checked-in, Cancelled, No-Show, Completed
        public string Status { get; set; } = "Confirmed";

        public decimal? ForfeitedAmount { get; set; }
        public decimal? RefundAmount { get; set; }
        public string RefundMethod { get; set; } = string.Empty;
        public string RefundStatus { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
