using Inventory_Management.Domain.Entities;

namespace Inventory_Management.Application.Common.Interfaces;

public interface IProductCategoryRepository
{
    Task<IEnumerable<ProductCategory>> GetAllAsync();
    Task<ProductCategory?> GetByIdAsync(Guid id);
    Task AddAsync(ProductCategory productCategory);
}
