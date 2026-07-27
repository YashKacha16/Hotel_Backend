using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hotel_Backend.Models;
using Hotel_Backend.Services;

namespace Hotel_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TablesController : ControllerBase
    {
        private readonly ITableService _tableService;

        public TablesController(ITableService tableService)
        {
            _tableService = tableService;
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

        [HttpGet("resolve/{qrToken}")]
        public async Task<ActionResult<RestaurantTableDto>> ResolveByQrToken(string qrToken)
        {
            var table = await _tableService.ResolveByQrTokenAsync(qrToken);
            if (table == null) return NotFound(new { message = "Invalid QR token." });
            if (table.Status == TableStatus.Cleaning) return StatusCode(409, new { message = "Table is currently unavailable." });

            var allTables = await _tableService.GetAllAsync(null);
            var groupCapacities = allTables
                .Where(t => t.MergeGroupId != null)
                .GroupBy(t => t.MergeGroupId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Capacity));

            return Ok(MapToDto(table, groupCapacities));
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
