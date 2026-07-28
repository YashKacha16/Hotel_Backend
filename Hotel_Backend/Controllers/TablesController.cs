using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hotel_Backend.Models;
using Hotel_Backend.Services;
using Hotel_Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TablesController : ControllerBase
    {
        private readonly ITableService _tableService;
        private readonly AppDbContext _context;

        public TablesController(ITableService tableService, AppDbContext context)
        {
            _tableService = tableService;
            _context = context;
        }

        public class UpdateStatusRequest
        {
            public string Status { get; set; } = string.Empty;
        }

        private RestaurantTableDto MapToDto(RestaurantTable table, Dictionary<int, int> groupCapacities)
        {
            return new RestaurantTableDto
            {
                Id = table.Id,
                Name = table.Name,
                Capacity = table.Capacity,
                Status = table.Status.ToString(),
                Position = table.Position,
                CategoryId = table.CategoryId,
                CategoryName = table.Category?.Name,
                IsMerged = table.MergeGroupId != null,
                MergeGroupId = table.MergeGroupId,
                CombinedCapacity = table.MergeGroupId != null && groupCapacities.TryGetValue(table.MergeGroupId.Value, out var cap) ? cap : null,
                QrToken = table.QrToken
            };
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RestaurantTableDto>>> GetAll([FromQuery] int? categoryId)
        {
            var tables = await _tableService.GetAllAsync(categoryId);
            var allTables = await _tableService.GetAllAsync(null);
            var groupCapacities = allTables
                .Where(t => t.MergeGroupId != null)
                .GroupBy(t => t.MergeGroupId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Capacity));

            var dtos = tables.Select(t => MapToDto(t, groupCapacities));
            return Ok(dtos);
        }

        [HttpGet("grouped")]
        public async Task<ActionResult<IEnumerable<TableGroupedDto>>> GetGrouped()
        {
            var allTables = await _tableService.GetAllAsync(null);
            var groupCapacities = allTables
                .Where(t => t.MergeGroupId != null)
                .GroupBy(t => t.MergeGroupId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Capacity));

            var grouped = allTables
                .GroupBy(t => new { t.CategoryId, CategoryName = t.Category?.Name ?? "Unassigned" })
                .Select(g => new TableGroupedDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    Tables = g.OrderBy(t => t.Position).Select(t => MapToDto(t, groupCapacities)).ToList()
                })
                .ToList();

            return Ok(grouped);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RestaurantTableDto>> GetById(int id)
        {
            var table = await _tableService.GetByIdAsync(id);
            if (table == null) return NotFound();

            var allTables = await _tableService.GetAllAsync(null);
            var groupCapacities = allTables
                .Where(t => t.MergeGroupId != null)
                .GroupBy(t => t.MergeGroupId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Capacity));

            return Ok(MapToDto(table, groupCapacities));
        }

        [HttpPost]
        public async Task<ActionResult<RestaurantTableDto>> Create([FromBody] CreateTableDto dto)
        {
            try
            {
                var created = await _tableService.AddAsync(dto);
                var reloaded = await _tableService.GetByIdAsync(created.Id);
                if (reloaded == null) return BadRequest();

                var allTables = await _tableService.GetAllAsync(null);
                var groupCapacities = allTables
                    .Where(t => t.MergeGroupId != null)
                    .GroupBy(t => t.MergeGroupId!.Value)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Capacity));

                return CreatedAtAction(nameof(GetById), new { id = reloaded.Id }, MapToDto(reloaded, groupCapacities));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTableDto dto)
        {
            try
            {
                var updated = await _tableService.UpdateAsync(id, dto);
                if (updated == null) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _tableService.DeleteAsync(id);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            if (!Enum.TryParse<TableStatus>(request.Status, true, out var statusEnum))
            {
                return BadRequest(new { message = $"Invalid status value '{request.Status}'." });
            }

            var updated = await _tableService.UpdateStatusAsync(id, statusEnum, "User");
            if (updated == null) return NotFound();
            return NoContent();
        }

        [HttpPatch("reorder")]
        public async Task<IActionResult> Reorder([FromBody] List<TableReorderItemDto> reorderList)
        {
            var success = await _tableService.ReorderAsync(reorderList);
            if (!success) return BadRequest(new { message = "Reorder operation failed." });
            return NoContent();
        }

        [HttpPost("merge")]
        public async Task<ActionResult<IEnumerable<RestaurantTableDto>>> Merge([FromBody] MergeRequestDto request)
        {
            try
            {
                var updatedTables = await _tableService.MergeTablesAsync(request.TableIds, "User");
                
                var allTables = await _tableService.GetAllAsync(null);
                var groupCapacities = allTables
                    .Where(t => t.MergeGroupId != null)
                    .GroupBy(t => t.MergeGroupId!.Value)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Capacity));

                var dtos = updatedTables.Select(t => MapToDto(t, groupCapacities));
                return Ok(dtos);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{mergeGroupId}/unmerge")]
        public async Task<IActionResult> Unmerge(int mergeGroupId)
        {
            var success = await _tableService.UnmergeAsync(mergeGroupId);
            if (!success) return NotFound(new { message = $"Merge group {mergeGroupId} not found." });
            return Ok(new { message = "Tables successfully unmerged." });
        }

        [HttpPatch("bulk-status")]
        public async Task<IActionResult> BulkUpdateStatus([FromBody] BulkStatusRequestDto request)
        {
            if (!Enum.TryParse<TableStatus>(request.Status, true, out var statusEnum))
            {
                return BadRequest(new { message = $"Invalid status value '{request.Status}'." });
            }

            var success = await _tableService.BulkUpdateStatusAsync(request.TableIds, statusEnum, "User");
            if (!success) return BadRequest(new { message = "Bulk status update failed or no tables provided." });
            return NoContent();
        }

        [HttpPatch("bulk-category")]
        public async Task<IActionResult> BulkUpdateCategory([FromBody] BulkCategoryRequestDto request)
        {
            var success = await _tableService.BulkUpdateCategoryAsync(request.TableIds, request.CategoryId);
            if (!success) return BadRequest(new { message = "Bulk category update failed or no tables provided." });
            return NoContent();
        }

        [HttpDelete("bulk")]
        public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteRequestDto request)
        {
            var skippedIds = await _tableService.BulkDeleteAsync(request.TableIds);
            return Ok(new { skippedIds });
        }

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<RestaurantTableDto>>> GetAvailable([FromQuery] int partySize, [FromQuery] string seating = "No preference")
        {
            var allTables = await _tableService.GetAllAsync(null);
            var groupCapacities = allTables
                .Where(t => t.MergeGroupId != null)
                .GroupBy(t => t.MergeGroupId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Capacity));

            // Available tables only: The table must be Free, and if it's merged, ALL tables in the merge group must be Free.
            var occupiedMergeGroups = allTables
                .Where(t => t.MergeGroupId != null && t.Status != TableStatus.Free)
                .Select(t => t.MergeGroupId!.Value)
                .Distinct()
                .ToHashSet();

            var available = allTables.Where(t => 
                t.Status == TableStatus.Free && 
                (t.MergeGroupId == null || !occupiedMergeGroups.Contains(t.MergeGroupId.Value))
            ).ToList();

            // Filter by capacity (check combined capacity if merged, else regular capacity)
            var suitable = available.Where(t =>
            {
                int cap = t.MergeGroupId != null && groupCapacities.TryGetValue(t.MergeGroupId.Value, out var groupCap)
                    ? groupCap
                    : t.Capacity;
                return cap >= partySize;
            }).ToList();

            // Filter by seating preference if it's not "No preference"
            if (!string.Equals(seating, "No preference", StringComparison.OrdinalIgnoreCase))
            {
                suitable = suitable.Where(t => t.Category != null &&
                    (t.Category.Name.Contains(seating, StringComparison.OrdinalIgnoreCase) ||
                     seating.Contains(t.Category.Name, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            // Sort by best fit (smallest capacity that fits the party)
            var sorted = suitable.OrderBy(t =>
            {
                int cap = t.MergeGroupId != null && groupCapacities.TryGetValue(t.MergeGroupId.Value, out var groupCap)
                    ? groupCap
                    : t.Capacity;
                return cap - partySize;
            });

            return Ok(sorted.Select(t => MapToDto(t, groupCapacities)));
        }

        [HttpPost("{id}/assign")]
        public async Task<IActionResult> AssignTable(int id, [FromBody] AssignTableDto request)
        {
            var table = await _tableService.GetByIdAsync(id);
            if (table == null) return NotFound(new { message = "Table not found." });
            if (table.Status != TableStatus.Free) return BadRequest(new { message = "Table is not available." });

            // Mark table as occupied
            await _tableService.UpdateStatusAsync(id, TableStatus.Occupied, "System");

            // Create an empty order for the table to represent the seating record
            var newOrder = new Order
            {
                OrderNumber = "ORD-" + DateTime.Now.ToString("HHmmssff"), // Temp simple generation
                Type = OrderType.DineIn,
                TableId = id,
                MergeGroupId = table.MergeGroupId,
                CustomerName = request.GuestName,
                SpecialInstructions = request.Notes,
                Status = OrderStatus.New,
                IsPriority = false,
                CreatedAt = DateTime.UtcNow,
                Subtotal = 0,
                HasNewAddOns = false
            };

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Table assigned successfully", orderId = newOrder.Id });
        }

        [HttpGet("resolve/{qrToken}")]
        public async Task<ActionResult<RestaurantTableDto>> ResolveByQrToken(string qrToken)
        {
            var table = await _tableService.ResolveByQrTokenAsync(qrToken);
            if (table == null) return NotFound(new { message = "Invalid QR token." });
            if (table.Status == TableStatus.Cleaning) return StatusCode(409, new { message = "Table is currently unavailable." });

            // Block access if a bill has already been generated
            Order? existingOrder = null;
            if (table.MergeGroupId.HasValue)
            {
                existingOrder = await _context.Orders
                    .Where(o => o.MergeGroupId == table.MergeGroupId.Value)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();
            }
            else
            {
                existingOrder = await _context.Orders
                    .Where(o => o.TableId == table.Id)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();
            }

            var allTables = await _tableService.GetAllAsync(null);
            var groupCapacities = allTables
                .Where(t => t.MergeGroupId != null)
                .GroupBy(t => t.MergeGroupId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Capacity));

            var dto = MapToDto(table, groupCapacities);

            if (existingOrder != null)
            {
                var existingBill = await _context.RestaurantBills.FirstOrDefaultAsync(b => b.OrderId == existingOrder.Id);
                if (existingBill != null && existingBill.Status != BillStatus.Paid)
                {
                    return StatusCode(409, new { message = "A bill has already been generated for this table. Please pay the bill before placing new orders." });
                }
                else if (existingBill == null)
                {
                    // Order is active and has no bill yet, return it so the UI can resume
                    dto.ActiveOrderId = existingOrder.Id;
                    dto.ActiveOrderNumber = existingOrder.OrderNumber;
                    dto.ActiveOrderStatus = existingOrder.Status.ToString();
                }
            }

            return Ok(dto);
        }

        [HttpGet("{id}/qr-image")]
        public async Task<IActionResult> GetQrImage(int id)
        {
            try
            {
                var pngBytes = await _tableService.GetQrImageAsync(id);
                return File(pngBytes, "image/png");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/regenerate-qr")]
        public async Task<ActionResult<RestaurantTableDto>> RegenerateQr(int id)
        {
            var table = await _tableService.RegenerateQrTokenAsync(id);
            if (table == null) return NotFound();

            var allTables = await _tableService.GetAllAsync(null);
            var groupCapacities = allTables
                .Where(t => t.MergeGroupId != null)
                .GroupBy(t => t.MergeGroupId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Capacity));

            return Ok(MapToDto(table, groupCapacities));
        }
    }
}
