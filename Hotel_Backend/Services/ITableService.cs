using System.Collections.Generic;
using System.Threading.Tasks;
using Hotel_Backend.Models;

namespace Hotel_Backend.Services
{
    public interface ITableService
    {
        Task<IEnumerable<RestaurantTable>> GetAllAsync(int? categoryId);
        Task<RestaurantTable?> GetByIdAsync(int id);
        Task<RestaurantTable> AddAsync(CreateTableDto dto);
        Task<RestaurantTable?> UpdateAsync(int id, UpdateTableDto dto);
        Task<bool> DeleteAsync(int id);
        Task<RestaurantTable?> UpdateStatusAsync(int id, TableStatus status, string changedBy);
        Task<bool> ReorderAsync(List<TableReorderItemDto> reorderList);
        Task<IEnumerable<RestaurantTable>> MergeTablesAsync(List<int> tableIds, string mergedBy);
        Task<bool> UnmergeAsync(int mergeGroupId);
        Task<bool> BulkUpdateStatusAsync(List<int> tableIds, TableStatus status, string changedBy);
        Task<bool> BulkUpdateCategoryAsync(List<int> tableIds, int categoryId);
        Task<List<int>> BulkDeleteAsync(List<int> tableIds);
        Task<RestaurantTable?> RegenerateQrTokenAsync(int tableId);
        Task<RestaurantTable?> ResolveByQrTokenAsync(string token);
        Task<byte[]> GetQrImageAsync(int tableId);
    }
}
