using System;

namespace Hotel_Backend.Models
{
    public class BillSplit
    {
        public int Id { get; set; }
        
        public int RestaurantBillId { get; set; }
        public RestaurantBill? RestaurantBill { get; set; }

        public string SplitName { get; set; } = string.Empty; // e.g., "Split A"
        public decimal Amount { get; set; }
        
        public BillStatus Status { get; set; } = BillStatus.Pending;
        public string? PaymentMethod { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
