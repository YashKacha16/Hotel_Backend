using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hotel_Backend.Models
{
    public class RestaurantBill
    {
        public int Id { get; set; }

        [Required]
        public string BillNumber { get; set; } = string.Empty;

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxPercent { get; set; } = 18m;
        public decimal CgstPercent { get; set; } = 9m;
        public decimal SgstPercent { get; set; } = 9m;
        public decimal ServiceCharge { get; set; }
        public decimal ServiceChargePercent { get; set; } = 10m;
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }

        public string? PaymentMethod { get; set; }
        
        public BillStatus Status { get; set; } = BillStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }

        public ICollection<BillSplit> Splits { get; set; } = new List<BillSplit>();
    }
}
