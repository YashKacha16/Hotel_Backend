using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_Backend.Models
{
    public class RoomBill
    {
        [Key]
        public int Id { get; set; }

        public string BillNumber { get; set; } = string.Empty;

        public int BookingId { get; set; }
        [ForeignKey("BookingId")]
        public Booking? Booking { get; set; }

        public DateTime CheckInDateTime { get; set; }
        public DateTime CheckOutDateTime { get; set; }

        public int BilledNights { get; set; }
        public decimal RoomPricePerNight { get; set; }
        public decimal TotalRoomAmount { get; set; }
        
        public decimal TotalRestaurantAmount { get; set; }
        public decimal AdvanceAmount { get; set; }
        
        public decimal TotalAmount { get; set; }
        public decimal DueAmount { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = "Paid"; // Pending, Paid

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
