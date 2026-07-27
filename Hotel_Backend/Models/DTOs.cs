using System.Collections.Generic;

namespace Hotel_Backend.Models
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Position { get; set; }
        public bool IsActive { get; set; }
    }

    public class CategoryPositionDto
    {
        public int Id { get; set; }
        public int Position { get; set; }
    }

    public class MenuItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public string? Image { get; set; }
        public bool Veg { get; set; }
        public bool Available { get; set; }
        public int Position { get; set; }
    }

    public class MenuGroupedDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int Position { get; set; }
        public List<MenuItemDto> Items { get; set; } = new List<MenuItemDto>();
    }

    public class RestaurantTableDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string Status { get; set; } = string.Empty;
        public int Position { get; set; }
        public bool IsMerged { get; set; }
        public int? MergeGroupId { get; set; }
        public int? CombinedCapacity { get; set; }
        public string QrToken { get; set; } = string.Empty;
    }

    public class PayBillDto
    {
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class CreateTableDto
    {
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int? CategoryId { get; set; }
    }

    public class UpdateTableDto
    {
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int? CategoryId { get; set; }
    }

    public class TableReorderItemDto
    {
        public int Id { get; set; }
        public int Position { get; set; }
    }

    public class MergeRequestDto
    {
        public List<int> TableIds { get; set; } = new List<int>();
    }

    public class BulkStatusRequestDto
    {
        public List<int> TableIds { get; set; } = new List<int>();
        public string Status { get; set; } = string.Empty;
    }

    public class BulkCategoryRequestDto
    {
        public List<int> TableIds { get; set; } = new List<int>();
        public int CategoryId { get; set; }
    }

    public class BulkDeleteRequestDto
    {
        public List<int> TableIds { get; set; } = new List<int>();
    }

    public class TableGroupedDto
    {
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<RestaurantTableDto> Tables { get; set; } = new List<RestaurantTableDto>();
    }

    public class CreateOrderItemDto
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal PriceAtOrder { get; set; }
        public bool IsAddOn { get; set; }
    }

    public class CreateOrderDto
    {
        public string Type { get; set; } = string.Empty; // DineIn, RoomService, Parcel
        public int? TableId { get; set; }
        public int? MergeGroupId { get; set; }
        public string? RoomNumber { get; set; }
        public string? CustomerName { get; set; }
        public bool IsPriority { get; set; }
        public string? SpecialInstructions { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();
    }

    public class UpdateOrderItemDto
    {
        public int? Id { get; set; } // If null, it's a new item (Add-on)
        public int MenuItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal PriceAtOrder { get; set; }
        public string Status { get; set; } = string.Empty; // Active, Cancelled
        public bool IsAddOn { get; set; }
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int? TableId { get; set; }
        public string? TableName { get; set; }
        public int? MergeGroupId { get; set; }
        public string? RoomNumber { get; set; }
        public string? ParcelCode { get; set; }
        public string? CustomerName { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsPriority { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Subtotal { get; set; }
        public bool HasNewAddOns { get; set; }
        public string? SpecialInstructions { get; set; }
        public int? BillId { get; set; }
        public string? BillStatus { get; set; }
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int MenuItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal PriceAtOrder { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsAddOn { get; set; }
    }

    public class KanbanOrdersDto
    {
        public List<OrderDto> New { get; set; } = new List<OrderDto>();
        public List<OrderDto> Preparing { get; set; } = new List<OrderDto>();
        public List<OrderDto> Ready { get; set; } = new List<OrderDto>();
        public List<OrderDto> Served { get; set; } = new List<OrderDto>();
    }

    public class BillSplitDto
    {
        public int Id { get; set; }
        public string SplitName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PaymentMethod { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class RestaurantBillDto
    {
        public int Id { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxPercent { get; set; } = 18m;
        public decimal ServiceCharge { get; set; }
        public decimal ServiceChargePercent { get; set; } = 10m;
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public List<BillSplitDto> Splits { get; set; } = new List<BillSplitDto>();
        public OrderDto? Order { get; set; }
    }

    public class CreateBillDto
    {
        public int OrderId { get; set; }
        public decimal? ServiceChargePercent { get; set; }
        public decimal? TaxPercent { get; set; }
        public decimal? Discount { get; set; }
    }

    public class UpdateBillDto
    {
        public decimal? ServiceChargePercent { get; set; }
        public decimal? TaxPercent { get; set; }
        public decimal? Discount { get; set; }
    }
}
