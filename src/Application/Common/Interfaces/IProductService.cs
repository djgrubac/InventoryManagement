using InventoryManagement.Core.DTO;

namespace InventoryManagement.Core.Interfaces;

public interface IProductService
{
    Task<Guid> CreateProductAsync(string name, decimal price, int stockQuantity);
    Task<IEnumerable<Products>> GetAllProductsAsync();
    Task<Products?> GetProductByIdAsync(Guid id);
    Task UpdateProductAsync(Guid id, string name, decimal price, int stockQuantity);
    Task DeleteProductAsync(Guid id);
}
