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
        public int? ActiveOrderId { get; set; }
        public string? ActiveOrderNumber { get; set; }
        public string? ActiveOrderStatus { get; set; }
    }

    public class PayBillDto
    {
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class AssignTableDto
    {
        public string GuestName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int PartySize { get; set; }
        public string? Notes { get; set; }
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
        public decimal CgstPercent { get; set; } = 9m;
        public decimal SgstPercent { get; set; } = 9m;
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
        public decimal? CgstPercent { get; set; }
        public decimal? SgstPercent { get; set; }
        public decimal? Discount { get; set; }
    }

    public class UpdateBillDto
    {
        public decimal? ServiceChargePercent { get; set; }
        public decimal? TaxPercent { get; set; }
        public decimal? CgstPercent { get; set; }
        public decimal? SgstPercent { get; set; }
        public decimal? Discount { get; set; }
    }

    public class SettingsDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Currency { get; set; } = "INR (₹)";
        public decimal ServiceChargePercent { get; set; } = 0;
        public decimal CgstPercent { get; set; } = 0;
        public decimal SgstPercent { get; set; } = 0;
        public int WaitlistEstimatedWaitMinutes { get; set; } = 22;
        public string WaitlistMessage { get; set; } = "Based on average turnover of 48m over the last hour and 3 free tables.";
    }

    public class WaitlistDto
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int PartySize { get; set; }
        public string? SeatingPreference { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public System.DateTime CreatedAt { get; set; }
        public int WaitedMin { get; set; }
        public int? AssignedTableId { get; set; }
        public string? AssignedTableName { get; set; }
    }

    public class CreateWaitlistDto
    {
        public string GuestName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int PartySize { get; set; }
        public string? SeatingPreference { get; set; }
        public string? Notes { get; set; }
    }

    // ── Room Category DTOs ──────────────────────────────────────

    public class RoomCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string Currency { get; set; } = "INR";
        public bool SeasonalPricingEnabled { get; set; }
        public bool IsActive { get; set; }
        public int? Capacity { get; set; }
        public string? Amenities { get; set; }
        public string? ImageUrl { get; set; }
        public int SeasonalRuleCount { get; set; }
    }

    public class CreateRoomCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string Currency { get; set; } = "INR";
        public bool SeasonalPricingEnabled { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public int? Capacity { get; set; }
        public string? Amenities { get; set; }
    }

    public class UpdateRoomCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string Currency { get; set; } = "INR";
        public bool SeasonalPricingEnabled { get; set; }
        public bool IsActive { get; set; }
        public int? Capacity { get; set; }
        public string? Amenities { get; set; }
    }

    // ── Seasonal Rule DTOs ──────────────────────────────────────

    public class SeasonalRuleDto
    {
        public int Id { get; set; }
        public int RoomCategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsRecurring { get; set; }
        public string? DaysOfWeek { get; set; }
        public decimal PriceModifierPercent { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateSeasonalRuleDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsRecurring { get; set; } = false;
        public string? DaysOfWeek { get; set; }
        public decimal PriceModifierPercent { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateSeasonalRuleDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsRecurring { get; set; }
        public string? DaysOfWeek { get; set; }
        public decimal PriceModifierPercent { get; set; }
        public bool IsActive { get; set; }
    }

    // ── Room DTOs ───────────────────────────────────────────────

    public class RoomDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public RoomCategoryDto? Category { get; set; }
        public string? Floor { get; set; }
        public int Capacity { get; set; }
        public decimal BasePrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> Amenities { get; set; } = new();
        public List<string> Images { get; set; } = new();
        public string? Description { get; set; }
    }

    public class CreateRoomDto
    {
        public string Number { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? Floor { get; set; }
        public int Capacity { get; set; }
        public decimal BasePrice { get; set; }
        public string Status { get; set; } = "Available";
        public List<string> Amenities { get; set; } = new();
        public List<string> Images { get; set; } = new();
        public string? Description { get; set; }
    }

    public class UpdateRoomDto
    {
        public string Number { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? Floor { get; set; }
        public int Capacity { get; set; }
        public decimal BasePrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> Amenities { get; set; } = new();
        public List<string> Images { get; set; } = new();
        public string? Description { get; set; }
    }

    // ── Booking DTOs ───────────────────────────────────────────────

    public class BookingDto
    {
        public int Id { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public string IdProofUrl { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public RoomDto? Room { get; set; }
        public DateTime CheckInDate { get; set; }
        public string CheckInTime { get; set; } = string.Empty;
        public DateTime CheckOutDate { get; set; }
        public string Source { get; set; } = string.Empty;
        public int Guests { get; set; }
        public decimal AdvanceAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal? ForfeitedAmount { get; set; }
        public decimal? RefundAmount { get; set; }
        public string RefundMethod { get; set; } = string.Empty;
        public string RefundStatus { get; set; } = string.Empty;
    }

    public class CreateBookingDto
    {
        public string GuestName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public string IdProofUrl { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public string CheckInTime { get; set; } = string.Empty;
        public DateTime CheckOutDate { get; set; }
        public string Source { get; set; } = "Online";
        public int Guests { get; set; }
        public decimal AdvanceAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = "Confirmed";
    }

    public class UpdateBookingDto
    {
        public string Status { get; set; } = string.Empty;
        public decimal? ForfeitedAmount { get; set; }
        public decimal? RefundAmount { get; set; }
        public string RefundMethod { get; set; } = string.Empty;
        public string RefundStatus { get; set; } = string.Empty;
    }
}
