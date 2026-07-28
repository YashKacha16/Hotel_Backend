using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hotel_Backend.Data;
using Hotel_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend.Services
{
    public class BillingService : IBillingService
    {
        private readonly AppDbContext _context;

        public BillingService(AppDbContext context)
        {
            _context = context;
        }

        private RestaurantBillDto MapToDto(RestaurantBill b)
        {
            return new RestaurantBillDto
            {
                Id = b.Id,
                BillNumber = b.BillNumber,
                OrderId = b.OrderId,
                Subtotal = b.Subtotal,
                TaxAmount = b.TaxAmount,
                TaxPercent = b.TaxPercent > 0 ? b.TaxPercent : 18m,
                CgstPercent = b.CgstPercent,
                SgstPercent = b.SgstPercent,
                ServiceCharge = b.ServiceCharge,
                ServiceChargePercent = b.ServiceChargePercent > 0 ? b.ServiceChargePercent : 10m,
                Discount = b.Discount,
                TotalAmount = b.TotalAmount,
                PaymentMethod = b.PaymentMethod,
                Status = b.Status.ToString(),
                CreatedAt = b.CreatedAt,
                PaidAt = b.PaidAt,
                Order = b.Order != null ? new OrderDto
                {
                    Id = b.Order.Id,
                    OrderNumber = b.Order.OrderNumber,
                    Type = b.Order.Type.ToString(),
                    TableId = b.Order.TableId,
                    TableName = b.Order.Table != null 
                        ? b.Order.Table.Name 
                        : (b.Order.MergeGroup != null && b.Order.MergeGroup.Tables.Any() 
                            ? string.Join("+", b.Order.MergeGroup.Tables.Select(t => t.Name).OrderBy(n => n)) 
                            : null),
                    CustomerName = b.Order.CustomerName,
                    RoomNumber = b.Order.RoomNumber,
                    Items = b.Order.Items.Select(i => new OrderItemDto
                    {
                        Id = i.Id,
                        MenuItemId = i.MenuItemId,
                        Name = i.Name,
                        Quantity = i.Quantity,
                        PriceAtOrder = i.PriceAtOrder,
                        IsAddOn = i.IsAddOn,
                        Status = i.Status.ToString()
                    }).ToList()
                } : null,
                Splits = b.Splits.Select(s => new BillSplitDto
                {
                    Id = s.Id,
                    SplitName = s.SplitName,
                    Amount = s.Amount,
                    Status = s.Status.ToString(),
                    PaymentMethod = s.PaymentMethod,
                    PaidAt = s.PaidAt
                }).ToList()
            };
        }

        public Task<RestaurantBillDto> GenerateBillAsync(int orderId)
        {
            return GenerateBillAsync(new CreateBillDto { OrderId = orderId });
        }

        public async Task<RestaurantBillDto> GenerateBillAsync(CreateBillDto dto)
        {
            var existingBill = await _context.RestaurantBills
                .Include(b => b.Splits)
                .Include(b => b.Order).ThenInclude(o => o.Table)
                .Include(b => b.Order).ThenInclude(o => o.MergeGroup).ThenInclude(g => g!.Tables)
                .Include(b => b.Order).ThenInclude(o => o.Items)
                .FirstOrDefaultAsync(b => b.OrderId == dto.OrderId);

            if (existingBill != null)
            {
                return MapToDto(existingBill);
            }

            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null) throw new ArgumentException("Order not found");

            decimal scPercent = dto.ServiceChargePercent ?? 10m;
            decimal cgstPercent = dto.CgstPercent ?? 9m;
            decimal sgstPercent = dto.SgstPercent ?? 9m;
            decimal taxPercent = dto.TaxPercent ?? (cgstPercent + sgstPercent);
            decimal discount = dto.Discount ?? 0m;

            // Calculate totals
            decimal subtotal = order.Items.Where(i => i.Status != OrderItemStatus.Cancelled).Sum(i => i.Quantity * i.PriceAtOrder);
            decimal serviceCharge = Math.Round(subtotal * (scPercent / 100m), 2);
            decimal taxAmount = Math.Round((subtotal + serviceCharge) * (taxPercent / 100m), 2);
            decimal totalAmount = Math.Max(0m, subtotal + serviceCharge + taxAmount - discount);

            // Generate BillNumber
            var nextId = (await _context.RestaurantBills.AnyAsync() ? await _context.RestaurantBills.MaxAsync(b => b.Id) : 0) + 1;
            var billNumber = $"BL-{2200 + nextId}";

            var bill = new RestaurantBill
            {
                BillNumber = billNumber,
                OrderId = order.Id,
                Subtotal = subtotal,
                ServiceCharge = serviceCharge,
                ServiceChargePercent = scPercent,
                TaxAmount = taxAmount,
                TaxPercent = taxPercent,
                CgstPercent = cgstPercent,
                SgstPercent = sgstPercent,
                Discount = discount,
                TotalAmount = totalAmount,
                Status = BillStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.RestaurantBills.Add(bill);
            await _context.SaveChangesAsync();
            
            // Reload with relations
            bill = await _context.RestaurantBills
                .Include(b => b.Splits)
                .Include(b => b.Order).ThenInclude(o => o.Table)
                .Include(b => b.Order).ThenInclude(o => o.MergeGroup).ThenInclude(g => g!.Tables)
                .Include(b => b.Order).ThenInclude(o => o.Items)
                .FirstAsync(b => b.Id == bill.Id);

            return MapToDto(bill);
        }

        public async Task<RestaurantBillDto?> UpdateBillAsync(int id, UpdateBillDto dto)
        {
            var bill = await _context.RestaurantBills
                .Include(b => b.Splits)
                .Include(b => b.Order).ThenInclude(o => o.Table)
                .Include(b => b.Order).ThenInclude(o => o.MergeGroup).ThenInclude(g => g!.Tables)
                .Include(b => b.Order).ThenInclude(o => o.Items)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bill == null) return null;
            if (bill.Status == BillStatus.Paid) throw new InvalidOperationException("Cannot modify a paid bill");

            if (dto.ServiceChargePercent.HasValue) bill.ServiceChargePercent = dto.ServiceChargePercent.Value;
            if (dto.CgstPercent.HasValue) bill.CgstPercent = dto.CgstPercent.Value;
            if (dto.SgstPercent.HasValue) bill.SgstPercent = dto.SgstPercent.Value;
            if (dto.TaxPercent.HasValue) bill.TaxPercent = dto.TaxPercent.Value;
            else if (dto.CgstPercent.HasValue || dto.SgstPercent.HasValue) bill.TaxPercent = bill.CgstPercent + bill.SgstPercent;
            
            if (dto.Discount.HasValue) bill.Discount = dto.Discount.Value;

            decimal subtotal = bill.Order != null 
                ? bill.Order.Items.Where(i => i.Status != OrderItemStatus.Cancelled).Sum(i => i.Quantity * i.PriceAtOrder)
                : bill.Subtotal;

            bill.Subtotal = subtotal;
            bill.ServiceCharge = Math.Round(subtotal * (bill.ServiceChargePercent / 100m), 2);
            bill.TaxAmount = Math.Round((subtotal + bill.ServiceCharge) * (bill.TaxPercent / 100m), 2);
            bill.TotalAmount = Math.Max(0m, subtotal + bill.ServiceCharge + bill.TaxAmount - bill.Discount);

            await _context.SaveChangesAsync();
            return MapToDto(bill);
        }

        public async Task<IEnumerable<RestaurantBillDto>> GetBillsAsync()
        {
            var bills = await _context.RestaurantBills
                .Include(b => b.Splits)
                .Include(b => b.Order).ThenInclude(o => o.Table)
                .Include(b => b.Order).ThenInclude(o => o.MergeGroup).ThenInclude(g => g!.Tables)
                .Include(b => b.Order).ThenInclude(o => o.Items)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return bills.Select(MapToDto);
        }

        public async Task<RestaurantBillDto?> GetBillByIdAsync(int id)
        {
            var bill = await _context.RestaurantBills
                .Include(b => b.Splits)
                .Include(b => b.Order).ThenInclude(o => o.Table)
                .Include(b => b.Order).ThenInclude(o => o.MergeGroup).ThenInclude(g => g!.Tables)
                .Include(b => b.Order).ThenInclude(o => o.Items)
                .FirstOrDefaultAsync(b => b.Id == id);

            return bill != null ? MapToDto(bill) : null;
        }

        public async Task<RestaurantBillDto?> MarkAsPaidAsync(int id, PayBillDto payBillDto)
        {
            var bill = await _context.RestaurantBills
                .Include(b => b.Splits)
                .Include(b => b.Order).ThenInclude(o => o.Table)
                .Include(b => b.Order).ThenInclude(o => o.MergeGroup).ThenInclude(g => g!.Tables)
                .Include(b => b.Order).ThenInclude(o => o.Items)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bill == null) return null;

            bill.Status = BillStatus.Paid;
            bill.PaymentMethod = payBillDto.PaymentMethod;
            bill.PaidAt = DateTime.UtcNow;

            if (bill.Order != null && bill.Order.Type == OrderType.DineIn)
            {
                if (bill.Order.MergeGroupId.HasValue)
                {
                    var mergeGroup = await _context.TableMergeGroups
                        .Include(g => g.Tables)
                        .FirstOrDefaultAsync(g => g.Id == bill.Order.MergeGroupId.Value);
                    if (mergeGroup != null)
                    {
                        foreach (var t in mergeGroup.Tables)
                        {
                            t.MergeGroupId = null;
                            t.Status = TableStatus.Free;
                            t.LastStatusChangedAt = DateTime.UtcNow;
                            t.LastStatusChangedBy = "Bill Paid";
                        }
                        
                        var orders = await _context.Orders
                            .Where(o => o.MergeGroupId == bill.Order.MergeGroupId.Value)
                            .ToListAsync();
                        foreach (var order in orders)
                        {
                            order.MergeGroupId = null;
                        }

                        _context.TableMergeGroups.Remove(mergeGroup);
                    }
                }
                else if (bill.Order.TableId.HasValue)
                {
                    var table = await _context.RestaurantTables.FindAsync(bill.Order.TableId.Value);
                    if (table != null)
                    {
                        table.Status = TableStatus.Free;
                        table.LastStatusChangedAt = DateTime.UtcNow;
                        table.LastStatusChangedBy = "Bill Paid";
                    }
                }
            }

            await _context.SaveChangesAsync();
            return MapToDto(bill);
        }
    }
}
