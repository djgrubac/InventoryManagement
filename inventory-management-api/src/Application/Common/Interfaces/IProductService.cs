using Inventory_Management.Application.Common.Models;

namespace InventoryManagement.Core.Interfaces;

public interface IProductService
{
    Task<Guid> CreateProductAsync(string name, decimal price, int stockQuantity, string description);
    Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
    Task<ProductDTO?> GetProductByUidAsync(Guid uid);
    Task UpdateProductAsync(Guid uid, string name, decimal price, int stockQuantity, string description);
    Task DeleteProductAsync(Guid uid);
}
