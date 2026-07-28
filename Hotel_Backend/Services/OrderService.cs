using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Hotel_Backend.Data;
using Hotel_Backend.Models;
using Hotel_Backend.Hubs;

namespace Hotel_Backend.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<KitchenHub> _hubContext;

        public OrderService(AppDbContext context, IHubContext<KitchenHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        private OrderDto MapToDto(Order o, RestaurantBill? bill = null)
        {
            string? tableName = null;
            if (o.Table != null)
            {
                tableName = o.Table.Name;
            }
            else if (o.MergeGroup != null && o.MergeGroup.Tables.Any())
            {
                var names = o.MergeGroup.Tables.Select(t => t.Name).OrderBy(n => n).ToList();
                tableName = string.Join("+", names);
            }

            var orderBill = bill ?? _context.RestaurantBills.FirstOrDefault(b => b.OrderId == o.Id);

            return new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Type = o.Type.ToString(),
                TableId = o.TableId,
                TableName = tableName,
                MergeGroupId = o.MergeGroupId,
                RoomNumber = o.RoomNumber,
                ParcelCode = o.ParcelCode,
                CustomerName = o.CustomerName,
                Status = o.Status.ToString(),
                IsPriority = o.IsPriority,
                CreatedAt = o.CreatedAt,
                Subtotal = o.Subtotal,
                HasNewAddOns = o.HasNewAddOns,
                SpecialInstructions = o.SpecialInstructions,
                BillId = orderBill?.Id,
                BillStatus = orderBill?.Status.ToString(),
                Items = o.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    MenuItemId = i.MenuItemId,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    PriceAtOrder = i.PriceAtOrder,
                    Status = i.Status.ToString(),
                    IsAddOn = i.IsAddOn
                }).ToList()
            };
        }

        public async Task<KanbanOrdersDto> GetKanbanOrdersAsync(string type)
        {
            string sanitizedType = (type ?? "").Replace("-", "").Replace(" ", "");
            if (!Enum.TryParse<OrderType>(sanitizedType, true, out var orderType))
            {
                return new KanbanOrdersDto();
            }

            var cutoff = DateTime.UtcNow.AddDays(-1);

            // Auto-delete orders older than 1 day (24 hours)
            var oldOrders = await _context.Orders
                .Where(o => o.CreatedAt < cutoff)
                .ToListAsync();
            if (oldOrders.Any())
            {
                _context.Orders.RemoveRange(oldOrders);
                await _context.SaveChangesAsync();
            }

            var activeOrders = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.MergeGroup).ThenInclude(g => g!.Tables)
                .Include(o => o.Items)
                .Where(o => o.Type == orderType && o.CreatedAt >= cutoff)
                .ToListAsync();

            var activeOrderIds = activeOrders.Select(o => o.Id).ToList();
            var bills = await _context.RestaurantBills
                .Where(b => activeOrderIds.Contains(b.OrderId))
                .ToListAsync();

            var billLookup = bills.ToDictionary(b => b.OrderId);

            var dtos = activeOrders.Select(o => MapToDto(o, billLookup.TryGetValue(o.Id, out var b) ? b : null)).ToList();

            return new KanbanOrdersDto
            {
                New = dtos.Where(d => d.Status == "New").ToList(),
                Preparing = dtos.Where(d => d.Status == "Preparing").ToList(),
                Ready = dtos.Where(d => d.Status == "Ready").ToList(),
                Served = dtos.Where(d => d.Status == "Served").ToList()
            };
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            if (!Enum.TryParse<OrderType>(dto.Type, true, out var orderType))
            {
                throw new ArgumentException("Invalid order type.");
            }

            // Generate OrderNumber
            var nextId = (await _context.Orders.AnyAsync() ? await _context.Orders.MaxAsync(o => o.Id) : 0) + 1;
            var orderNumber = $"ORD-{nextId:D4}";

            string? parcelCode = null;
            if (orderType == OrderType.Parcel)
            {
                var seq = await _context.ParcelCodeSequences.FirstOrDefaultAsync();
                if (seq == null)
                {
                    seq = new ParcelCodeSequence { LastNumber = 200 };
                    _context.ParcelCodeSequences.Add(seq);
                }
                seq.LastNumber += 1;
                await _context.SaveChangesAsync();
                parcelCode = $"PCL-{seq.LastNumber}";
            }

            var subtotal = dto.Items.Sum(i => i.Quantity * i.PriceAtOrder);

            if (orderType == OrderType.DineIn)
            {
                Order? existingOrder = null;
                if (dto.MergeGroupId.HasValue)
                {
                    existingOrder = await _context.Orders
                        .Include(o => o.Items)
                        .Where(o => o.MergeGroupId == dto.MergeGroupId.Value)
                        .OrderByDescending(o => o.CreatedAt)
                        .FirstOrDefaultAsync();
                }
                else if (dto.TableId.HasValue)
                {
                    existingOrder = await _context.Orders
                        .Include(o => o.Items)
                        .Where(o => o.TableId == dto.TableId.Value)
                        .OrderByDescending(o => o.CreatedAt)
                        .FirstOrDefaultAsync();
                }

                if (existingOrder != null)
                {
                    var existingBill = await _context.RestaurantBills.FirstOrDefaultAsync(b => b.OrderId == existingOrder.Id);
                    
                    if (existingBill == null)
                    {
                        foreach (var item in dto.Items)
                        {
                            existingOrder.Items.Add(new OrderItem
                            {
                                MenuItemId = item.MenuItemId,
                                Name = item.Name,
                                Quantity = item.Quantity,
                                PriceAtOrder = item.PriceAtOrder,
                                IsAddOn = true,
                                Status = OrderItemStatus.Active
                            });
                        }

                        existingOrder.Subtotal += subtotal;
                        existingOrder.HasNewAddOns = true;
                        
                        if (!string.IsNullOrWhiteSpace(dto.SpecialInstructions))
                        {
                            existingOrder.SpecialInstructions = string.IsNullOrWhiteSpace(existingOrder.SpecialInstructions)
                                ? dto.SpecialInstructions
                                : existingOrder.SpecialInstructions + " | " + dto.SpecialInstructions;
                        }

                        await _context.SaveChangesAsync();

                        var reloadedExisting = await _context.Orders
                            .Include(o => o.Table)
                            .Include(o => o.MergeGroup).ThenInclude(g => g!.Tables)
                            .Include(o => o.Items)
                            .FirstAsync(o => o.Id == existingOrder.Id);

                        var mappedExisting = MapToDto(reloadedExisting);
                        await _hubContext.Clients.Group("kitchen-display").SendAsync("OrderUpdated", mappedExisting);
                        
                        return mappedExisting;
                    }
                    else if (existingBill.Status != BillStatus.Paid)
                    {
                        throw new InvalidOperationException("A bill has already been generated for this table. Please pay the bill before placing new orders.");
                    }
                }
            }

            var order = new Order
            {
                OrderNumber = orderNumber,
                Type = orderType,
                TableId = dto.TableId,
                MergeGroupId = dto.MergeGroupId,
                RoomNumber = dto.RoomNumber,
                ParcelCode = parcelCode,
                CustomerName = dto.CustomerName,
                IsPriority = dto.IsPriority,
                CreatedAt = DateTime.UtcNow,
                Subtotal = subtotal,
                SpecialInstructions = dto.SpecialInstructions,
                HasNewAddOns = false,
                Status = OrderStatus.New
            };

            foreach (var item in dto.Items)
            {
                order.Items.Add(new OrderItem
                {
                    MenuItemId = item.MenuItemId,
                    Name = item.Name,
                    Quantity = item.Quantity,
                    PriceAtOrder = item.PriceAtOrder,
                    IsAddOn = item.IsAddOn,
                    Status = OrderItemStatus.Active
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Set table status to Occupied for Dine-in orders
            if (orderType == OrderType.DineIn)
            {
                if (dto.MergeGroupId.HasValue)
                {
                    var tables = await _context.RestaurantTables
                        .Where(t => t.MergeGroupId == dto.MergeGroupId.Value)
                        .ToListAsync();
                    foreach (var t in tables)
                    {
                        t.Status = TableStatus.Occupied;
                        t.LastStatusChangedAt = DateTime.UtcNow;
                        t.LastStatusChangedBy = "Order Started";
                    }
                    await _context.SaveChangesAsync();
                }
                else if (dto.TableId.HasValue)
                {
                    var table = await _context.RestaurantTables.FindAsync(dto.TableId.Value);
                    if (table != null)
                    {
                        table.Status = TableStatus.Occupied;
                        table.LastStatusChangedAt = DateTime.UtcNow;
                        table.LastStatusChangedBy = "Order Started";
                        await _context.SaveChangesAsync();
                    }
                }
            }

            // Reload to map correctly with tables details
            var reloaded = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.MergeGroup).ThenInclude(g => g!.Tables)
                .Include(o => o.Items)
                .FirstAsync(o => o.Id == order.Id);

            var mapped = MapToDto(reloaded);

            // Push to SignalR
            await _hubContext.Clients.Group("kitchen-display").SendAsync("NewOrder", mapped);

            return mapped;
        }

        public async Task<OrderDto?> UpdateOrderStatusAsync(int id, OrderStatus status)
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.MergeGroup).ThenInclude(g => g!.Tables)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return null;

            order.Status = status;
            await _context.SaveChangesAsync();

            var mapped = MapToDto(order);

            // Push to SignalR
            await _hubContext.Clients.Group("kitchen-display").SendAsync("OrderStatusChanged", id, status.ToString());

            return mapped;
        }

        public async Task<OrderDto?> AcknowledgeAddOnsAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.MergeGroup).ThenInclude(g => g!.Tables)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return null;

            order.HasNewAddOns = false;
            await _context.SaveChangesAsync();

            var mapped = MapToDto(order);

            // Push to SignalR
            await _hubContext.Clients.Group("kitchen-display").SendAsync("OrderUpdated", mapped);

            return mapped;
        }

        public async Task<OrderDto?> UpdateOrderItemsAsync(int id, List<UpdateOrderItemDto> items)
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.MergeGroup).ThenInclude(g => g!.Tables)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return null;

            bool addedNewItems = false;

            // Track active ids to remove items that aren't sent? Usually edit means adding new ones or cancelling.
            foreach (var item in items)
            {
                if (!item.Id.HasValue || item.Id == 0)
                {
                    // New item -> Add-on
                    order.Items.Add(new OrderItem
                    {
                        MenuItemId = item.MenuItemId,
                        Name = item.Name,
                        Quantity = item.Quantity,
                        PriceAtOrder = item.PriceAtOrder,
                        IsAddOn = true,
                        Status = OrderItemStatus.Active
                    });
                    addedNewItems = true;
                }
                else
                {
                    // Existing item
                    var existing = order.Items.FirstOrDefault(i => i.Id == item.Id.Value);
                    if (existing != null)
                    {
                        existing.Quantity = item.Quantity;
                        if (Enum.TryParse<OrderItemStatus>(item.Status, true, out var itemStatus))
                        {
                            existing.Status = itemStatus;
                        }
                    }
                }
            }

            if (addedNewItems)
            {
                order.HasNewAddOns = true;
                if (order.Status == OrderStatus.Ready || order.Status == OrderStatus.Served)
                {
                    order.Status = OrderStatus.New;
                }
            }

            // Re-calculate subtotal (excluding cancelled items)
            order.Subtotal = order.Items
                .Where(i => i.Status == OrderItemStatus.Active)
                .Sum(i => i.Quantity * i.PriceAtOrder);

            await _context.SaveChangesAsync();

            var mapped = MapToDto(order);

            // Push to SignalR
            await _hubContext.Clients.Group("kitchen-display").SendAsync("OrderUpdated", mapped);

            return mapped;
        }

        public async Task<OrderDto?> CancelOrderAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.MergeGroup).ThenInclude(g => g!.Tables)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return null;

            // Free table/merge group if Dine-in
            if (order.Type == OrderType.DineIn)
            {
                if (order.MergeGroupId.HasValue)
                {
                    var tables = await _context.RestaurantTables
                        .Where(t => t.MergeGroupId == order.MergeGroupId.Value)
                        .ToListAsync();
                    foreach (var t in tables)
                    {
                        t.Status = TableStatus.Free;
                        t.LastStatusChangedAt = DateTime.UtcNow;
                        t.LastStatusChangedBy = "Order Cancelled";
                    }
                }
                else if (order.TableId.HasValue)
                {
                    var table = await _context.RestaurantTables.FindAsync(order.TableId.Value);
                    if (table != null)
                    {
                        table.Status = TableStatus.Free;
                        table.LastStatusChangedAt = DateTime.UtcNow;
                        table.LastStatusChangedBy = "Order Cancelled";
                    }
                }
            }

            // Remove order or cancel items. Set subtotal to 0 and status Served so it clears from active kanban board columns
            order.Status = OrderStatus.Served;
            order.Subtotal = 0;
            foreach (var item in order.Items)
            {
                item.Status = OrderItemStatus.Cancelled;
            }

            await _context.SaveChangesAsync();

            var mapped = MapToDto(order);

            // Push to SignalR
            await _hubContext.Clients.Group("kitchen-display").SendAsync("OrderUpdated", mapped);

            return mapped;
        }
    }
}
