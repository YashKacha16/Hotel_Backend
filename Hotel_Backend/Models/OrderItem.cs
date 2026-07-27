namespace Hotel_Backend.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int MenuItemId { get; set; }
        public string Name { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal PriceAtOrder { get; set; }

        public OrderItemStatus Status { get; set; } = OrderItemStatus.Active;

        public bool IsAddOn { get; set; }
    }
}
