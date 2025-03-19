using Inventory_Management.Domain.Entities;

namespace Inventory_Management.Application.Common.Interfaces;

public interface IProductCategoryRepository
{
    Task<IEnumerable<ProductCategory>> GetAllAsync();
}
