using Inventory_Management.Application.Common.Models;

namespace Inventory_Management.Application.Common.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<ProductCategory>> GetAllAsync();
    Task<Guid> CreateCategoryAsync(Guid id, string caption);
}
