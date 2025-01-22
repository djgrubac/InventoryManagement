using InventoryManagement.Core.DTO;
using Inventory_Management.Domain.Entities;
using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Product;

public class ProductService: IProductService
{
    private readonly IBaseRepository<Entities.Product> _productRepository;

    public ProductService(IBaseRepository<Entities.Product> productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
    }
    
    public async Task<Guid> CreateProductAsync(string name, decimal price, int stockQuantity, string description)
    {
        var product = new Entities.Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Price = price,
            StockQuantity = stockQuantity,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
        await _productRepository.AddAsync(product);
        return product.Id;
    }

    public async Task<IEnumerable<Products>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(product => new Products
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Description = product.Description
        });
    }

    public async Task<Products?> GetProductByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if(product == null)
            return null;
        
        return new Products
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Description = product.Description
        };
    }

    public async Task UpdateProductAsync(Guid id, string name, decimal price, int stockQuantity, string description)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {id} not found.");
        
        product.Name = name;
        product.Price = price;
        product.StockQuantity = stockQuantity;
        product.Description = description;
        
        await _productRepository.UpdateAsync(product);
    }

    public async Task DeleteProductAsync(Guid id)
    {
        await _productRepository.DeleteAsync(id);
    }
}
