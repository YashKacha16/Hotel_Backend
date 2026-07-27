using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Hotel_Backend.Data;
using Hotel_Backend.Models;
using QRCoder;
using Microsoft.Extensions.Configuration;

namespace Hotel_Backend.Services
{
    public class TableService : ITableService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public TableService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IEnumerable<RestaurantTable>> GetAllAsync(int? categoryId)
        {
            var query = _context.RestaurantTables
                .Include(t => t.Category)
                .Include(t => t.MergeGroup)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == categoryId.Value);
            }

            return await query.OrderBy(t => t.Position).ToListAsync();
        }

        public async Task<RestaurantTable?> GetByIdAsync(int id)
        {
            return await _context.RestaurantTables
                .Include(t => t.Category)
                .Include(t => t.MergeGroup)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<RestaurantTable> AddAsync(CreateTableDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Table name is required.");
            }

            // 1. Validate Name is unique
            var exists = await _context.RestaurantTables.AnyAsync(t => t.Name == dto.Name);
            if (exists)
            {
                throw new ArgumentException($"Table name '{dto.Name}' already exists.");
            }

            // 2. Validate Capacity > 0
            if (dto.Capacity <= 0)
            {
                throw new ArgumentException("Capacity must be greater than zero.");
            }

            // 3. Compute next Position
            var maxPos = await _context.RestaurantTables
                .Where(t => t.CategoryId == dto.CategoryId)
                .MaxAsync(t => (int?)t.Position) ?? 0;

            var newTable = new RestaurantTable
                {
                    Name = dto.Name,
                    Capacity = dto.Capacity,
                    CategoryId = dto.CategoryId,
                    Status = TableStatus.Free,
                    Position = maxPos + 1,
                    QrToken = Guid.NewGuid().ToString("N"),
                    LastStatusChangedAt = DateTime.UtcNow,
                    LastStatusChangedBy = "System"
                };

            _context.RestaurantTables.Add(newTable);
            await _context.SaveChangesAsync();

            return newTable;
        }

        public async Task<RestaurantTable?> UpdateAsync(int id, UpdateTableDto dto)
        {
            var existing = await _context.RestaurantTables.FindAsync(id);
            if (existing == null) return null;

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Table name is required.");
            }

            if (existing.Name != dto.Name)
            {
                var exists = await _context.RestaurantTables.AnyAsync(t => t.Name == dto.Name && t.Id != id);
                if (exists)
                {
                    throw new ArgumentException($"Table name '{dto.Name}' already exists.");
                }
            }

            if (dto.Capacity <= 0)
            {
                throw new ArgumentException("Capacity must be greater than zero.");
            }

            existing.Name = dto.Name;
            existing.Capacity = dto.Capacity;
            existing.CategoryId = dto.CategoryId;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var table = await _context.RestaurantTables.FindAsync(id);
            if (table == null) return false;

            if (table.Status == TableStatus.Occupied)
            {
                throw new InvalidOperationException($"Cannot delete table {table.Name} because it is currently Occupied.");
            }

            _context.RestaurantTables.Remove(table);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<RestaurantTable?> UpdateStatusAsync(int id, TableStatus status, string changedBy)
        {
            var table = await _context.RestaurantTables.FindAsync(id);
            if (table == null) return null;

            if (status != TableStatus.Occupied && changedBy != "Bill Paid")
            {
                var hasUnpaidOrder = await _context.Orders.AnyAsync(o =>
                    o.Type == OrderType.DineIn &&
                    (table.MergeGroupId != null ? o.MergeGroupId == table.MergeGroupId : o.TableId == id) &&
                    !_context.RestaurantBills.Any(b => b.OrderId == o.Id && b.Status == BillStatus.Paid)
                );

                if (hasUnpaidOrder)
                {
                    throw new InvalidOperationException($"Cannot change status of Table '{table.Name}' because it has an active unpaid order.");
                }
            }

            table.Status = status;
            table.LastStatusChangedAt = DateTime.UtcNow;
            table.LastStatusChangedBy = changedBy;

            await _context.SaveChangesAsync();
            return table;
        }

        public async Task<bool> ReorderAsync(List<TableReorderItemDto> reorderList)
        {
            if (reorderList == null || !reorderList.Any()) return false;

            foreach (var item in reorderList)
            {
                var table = await _context.RestaurantTables.FindAsync(item.Id);
                if (table != null)
                {
                    table.Position = item.Position;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<RestaurantTable>> MergeTablesAsync(List<int> tableIds, string mergedBy)
        {
            if (tableIds == null || tableIds.Count < 2)
            {
                throw new ArgumentException("At least two tables must be specified to merge.");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                 {
                    var tables = await _context.RestaurantTables
                        .Where(t => tableIds.Contains(t.Id))
                        .ToListAsync();

                    if (tables.Count != tableIds.Count)
                    {
                        throw new KeyNotFoundException("One or more tables could not be found.");
                    }

                    var firstCategoryId = tables[0].CategoryId;
                    if (tables.Any(t => t.CategoryId != firstCategoryId))
                    {
                        throw new ArgumentException("Tables from different categories/zones cannot be merged.");
                    }

                    foreach (var table in tables)
                    {
                        if (table.MergeGroupId != null)
                        {
                            throw new InvalidOperationException($"Table {table.Name} is already merged.");
                        }

                        if (table.Status == TableStatus.Occupied)
                        {
                            throw new InvalidOperationException($"Table {table.Name} is currently Occupied and cannot be merged.");
                        }
                    }

                    var mergeGroup = new TableMergeGroup
                    {
                        MergedAt = DateTime.UtcNow,
                        MergedBy = mergedBy ?? "System"
                    };

                    _context.TableMergeGroups.Add(mergeGroup);
                    await _context.SaveChangesAsync();

                    foreach (var table in tables)
                    {
                        table.MergeGroupId = mergeGroup.Id;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return await _context.RestaurantTables
                        .Include(t => t.Category)
                        .Include(t => t.MergeGroup)
                        .Where(t => tableIds.Contains(t.Id))
                        .ToListAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<bool> UnmergeAsync(int mergeGroupId)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var mergeGroup = await _context.TableMergeGroups
                        .Include(g => g.Tables)
                        .FirstOrDefaultAsync(g => g.Id == mergeGroupId);

                    if (mergeGroup == null) return false;

                    foreach (var table in mergeGroup.Tables)
                    {
                        table.MergeGroupId = null;
                        table.Status = TableStatus.Free;
                    }

                    var orders = await _context.Orders
                        .Where(o => o.MergeGroupId == mergeGroupId)
                        .ToListAsync();
                    foreach (var order in orders)
                    {
                        order.MergeGroupId = null;
                    }

                    _context.TableMergeGroups.Remove(mergeGroup);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<bool> BulkUpdateStatusAsync(List<int> tableIds, TableStatus status, string changedBy)
        {
            if (tableIds == null || !tableIds.Any()) return false;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var id in tableIds)
                    {
                        await UpdateStatusAsync(id, status, changedBy);
                    }

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<bool> BulkUpdateCategoryAsync(List<int> tableIds, int categoryId)
        {
            if (tableIds == null || !tableIds.Any()) return false;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var tables = await _context.RestaurantTables
                        .Where(t => tableIds.Contains(t.Id))
                        .ToListAsync();

                    foreach (var table in tables)
                    {
                        table.CategoryId = categoryId;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<List<int>> BulkDeleteAsync(List<int> tableIds)
        {
            var skippedIds = new List<int>();
            if (tableIds == null || !tableIds.Any()) return skippedIds;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var tables = await _context.RestaurantTables
                        .Where(t => tableIds.Contains(t.Id))
                        .ToListAsync();

                    foreach (var table in tables)
                    {
                        if (table.Status == TableStatus.Occupied)
                        {
                            skippedIds.Add(table.Id);
                            continue;
                        }

                        _context.RestaurantTables.Remove(table);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return skippedIds;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<RestaurantTable?> RegenerateQrTokenAsync(int tableId)
        {
            var table = await _context.RestaurantTables.FindAsync(tableId);
            if (table == null) return null;

            table.QrToken = Guid.NewGuid().ToString("N");
            await _context.SaveChangesAsync();
            return table;
        }

        public async Task<RestaurantTable?> ResolveByQrTokenAsync(string token)
        {
            return await _context.RestaurantTables
                .Include(t => t.Category)
                .Include(t => t.MergeGroup)
                .FirstOrDefaultAsync(t => t.QrToken == token);
        }

        public async Task<byte[]> GetQrImageAsync(int tableId)
        {
            var table = await _context.RestaurantTables.FindAsync(tableId);
            if (table == null)
            {
                throw new KeyNotFoundException($"Table with ID {tableId} not found.");
            }

            var baseUrl = _configuration["QrBaseUrl"] ?? "https://yourdomain.com";
            var qrContent = $"{baseUrl.TrimEnd('/')}/order/{table.QrToken}";
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new PngByteQRCode(qrCodeData))
            {
                return qrCode.GetGraphic(20);
            }
        }
    }
}
