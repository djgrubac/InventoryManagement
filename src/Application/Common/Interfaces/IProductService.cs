using InventoryManagement.Core.DTO;

namespace InventoryManagement.Core.Interfaces;

public interface IProductService
{
    Task<Guid> CreateProductAsync(string name, decimal price, int stockQuantity, string description);
    Task<IEnumerable<Products>> GetAllProductsAsync();
    Task<Products?> GetProductByIdAsync(Guid id);
    Task UpdateProductAsync(Guid id, string name, decimal price, int stockQuantity, string description);
    Task DeleteProductAsync(Guid id);
}
