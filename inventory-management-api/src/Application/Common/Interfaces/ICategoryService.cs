using Inventory_Management.Application.Common.Models;

namespace Inventory_Management.Application.Common.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDTO>> GetAllAsync();
    Task<Guid> CreateCategoryAsync(Guid uid, string caption);
}
