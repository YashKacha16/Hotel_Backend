using System.Collections.Generic;
using System.Threading.Tasks;
using Hotel_Backend.Models;

namespace Hotel_Backend.Services
{
    public interface IMenuService
    {
        Task<IEnumerable<MenuItem>> GetAllMenuItemsAsync(int? categoryId = null);
        Task<MenuItem?> GetMenuItemByIdAsync(int id);
        Task<MenuItem> AddMenuItemAsync(MenuItem menuItem);
        Task<MenuItem?> UpdateMenuItemAsync(int id, MenuItem menuItem);
        Task<bool> DeleteMenuItemAsync(int id);
        Task<bool> ToggleAvailabilityAsync(int id);
        Task<bool> ReorderMenuItemsAsync(List<CategoryPositionDto> positions);
        Task<IEnumerable<MenuGroupedDto>> GetGroupedMenuAsync();
        Task<bool> UploadImageAsync(int id, Microsoft.AspNetCore.Http.IFormFile image);
    }
}
