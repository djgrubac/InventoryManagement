using Inventory_Management.Application.Common.Models;
using Inventory_Management.Domain.Entities;
using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Product;

public class ProductService: IProductService
{
    private readonly IBaseRepository<Entities.Product> _productRepository;

    public ProductService(IBaseRepository<Entities.Product> productRepository)
    {
        _productRepository = productRepository;
    }
    
    public async Task<Guid> CreateProductAsync(string name, decimal price, int stockQuantity, string description)
    {
        var product = new Entities.Product
        {
            Name = name,
            Price = price,
            StockQuantity = stockQuantity,
            Description = description,
        };
        await _productRepository.AddAsync(product);
        return product.Uid;
    }

    public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(product => new ProductDTO
        {
            Uid = product.Uid,
            Name = product.Name,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Description = product.Description
        });
    }

    public async Task<ProductDTO?> GetProductByUidAsync(Guid uid)
    {
        var product = await _productRepository.GetByUidAsync(uid);
        if(product == null)
            return null;
        
        return new ProductDTO
        {
            Uid = product.Uid,
            Name = product.Name,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Description = product.Description
        };
    }

    public async Task UpdateProductAsync(Guid uid, string name, decimal price, int stockQuantity, string description)
    {
        var product = await _productRepository.GetByUidAsync(uid);
        if (product == null)
            throw new KeyNotFoundException($"Product with Uid {uid} not found.");
        
        product.Name = name;
        product.Price = price;
        product.StockQuantity = stockQuantity;
        product.Description = description;
        
        await _productRepository.UpdateAsync(product);
    }

    public async Task DeleteProductAsync(Guid uid)
    {
        await _productRepository.DeleteAsync(uid);
    }
}
