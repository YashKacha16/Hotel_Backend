using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hotel_Backend.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string OrderNumber { get; set; } = string.Empty;

        public OrderType Type { get; set; }

        public int? TableId { get; set; }
        public RestaurantTable? Table { get; set; }

        public int? MergeGroupId { get; set; }
        public TableMergeGroup? MergeGroup { get; set; }

        public string? RoomNumber { get; set; }

        public string? ParcelCode { get; set; }

        public string? CustomerName { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.New;

        public bool IsPriority { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public decimal Subtotal { get; set; }

        public bool HasNewAddOns { get; set; }

        public string? SpecialInstructions { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
