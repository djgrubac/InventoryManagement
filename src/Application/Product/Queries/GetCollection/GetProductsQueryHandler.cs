using Inventory_Management.Application.Common.Interfaces;
using InventoryManagement.Core.DTO;
using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Product.Queries.GetCollection;

public class GetProductsQueryHandler:IRequestHandler<GetProductsQuery,IEnumerable<Products>>
{
    private readonly IBaseRepository<Entities.Product> _productRepository;

    public GetProductsQueryHandler(IBaseRepository<Entities.Product> productRepository)
    {
        _productRepository = productRepository;
    }
    
    public async Task<IEnumerable<Products>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(product=> new Products
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Description = product.Description
        });
    }
}
