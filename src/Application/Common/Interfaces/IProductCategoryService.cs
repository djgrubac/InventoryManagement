using Inventory_Management.Application.Common.Models;

namespace Inventory_Management.Application.Common.Interfaces;

public interface IProductCategoryService
{
    Task<IEnumerable<ProductCategory>> GetAllAsync();
    Task<Guid> CreateCategoryAsync(Guid id, string caption);
}
