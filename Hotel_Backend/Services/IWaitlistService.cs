using System.Collections.Generic;
using System.Threading.Tasks;
using Hotel_Backend.Models;

namespace Hotel_Backend.Services
{
    public interface IWaitlistService
    {
        Task<IEnumerable<WaitlistDto>> GetAllActiveAsync();
        Task<WaitlistDto> AddAsync(CreateWaitlistDto dto);
        Task<WaitlistDto> UpdateStatusAsync(int id, WaitlistStatus status);
        Task<WaitlistDto> AssignTableAsync(int id, int tableId);
    }
}
