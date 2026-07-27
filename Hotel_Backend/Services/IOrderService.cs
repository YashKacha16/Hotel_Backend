using System.Collections.Generic;
using System.Threading.Tasks;
using Hotel_Backend.Models;

namespace Hotel_Backend.Services
{
    public interface IOrderService
    {
        Task<KanbanOrdersDto> GetKanbanOrdersAsync(string type);
        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
        Task<OrderDto?> UpdateOrderStatusAsync(int id, OrderStatus status);
        Task<OrderDto?> AcknowledgeAddOnsAsync(int id);
        Task<OrderDto?> UpdateOrderItemsAsync(int id, List<UpdateOrderItemDto> items);
        Task<OrderDto?> CancelOrderAsync(int id);
    }
}
