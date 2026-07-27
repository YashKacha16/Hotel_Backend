using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hotel_Backend.Models;
using Hotel_Backend.Services;

namespace Hotel_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public class UpdateOrderStatusRequest
        {
            public string Status { get; set; } = string.Empty;
        }

        [HttpGet("kanban")]
        public async Task<ActionResult<KanbanOrdersDto>> GetKanban([FromQuery] string type)
        {
            var result = await _orderService.GetKanbanOrdersAsync(type);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto dto)
        {
            try
            {
                var result = await _orderService.CreateOrderAsync(dto);
                return CreatedAtAction(nameof(GetKanban), new { type = dto.Type }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult<OrderDto>> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var orderStatus))
            {
                return BadRequest(new { message = $"Invalid status value '{request.Status}'." });
            }

            var result = await _orderService.UpdateOrderStatusAsync(id, orderStatus);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpPost("{id}/acknowledge-addons")]
        public async Task<ActionResult<OrderDto>> AcknowledgeAddOns(int id)
        {
            var result = await _orderService.AcknowledgeAddOnsAsync(id);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpPut("{id}/items")]
        public async Task<ActionResult<OrderDto>> UpdateItems(int id, [FromBody] List<UpdateOrderItemDto> items)
        {
            var result = await _orderService.UpdateOrderItemsAsync(id, items);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<OrderDto>> Cancel(int id)
        {
            var result = await _orderService.CancelOrderAsync(id);
            if (result == null) return NotFound();

            return Ok(result);
        }
    }
}
