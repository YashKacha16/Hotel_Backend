using System.Collections.Generic;
using System.Threading.Tasks;
using Hotel_Backend.Models;

namespace Hotel_Backend.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<Category> AddCategoryAsync(Category category);
        Task<Category?> UpdateCategoryAsync(int id, Category category);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> ReorderCategoriesAsync(List<CategoryPositionDto> positions);
    }
}
