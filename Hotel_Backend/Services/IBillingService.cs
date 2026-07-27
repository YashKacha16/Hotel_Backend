using System.Collections.Generic;
using System.Threading.Tasks;
using Hotel_Backend.Models;

namespace Hotel_Backend.Services
{
    public interface IBillingService
    {
        Task<RestaurantBillDto> GenerateBillAsync(int orderId);
        Task<RestaurantBillDto> GenerateBillAsync(CreateBillDto dto);
        Task<RestaurantBillDto?> UpdateBillAsync(int id, UpdateBillDto dto);
        Task<IEnumerable<RestaurantBillDto>> GetBillsAsync();
        Task<RestaurantBillDto?> GetBillByIdAsync(int id);
        Task<RestaurantBillDto?> MarkAsPaidAsync(int id, PayBillDto payBillDto);
    }
}
