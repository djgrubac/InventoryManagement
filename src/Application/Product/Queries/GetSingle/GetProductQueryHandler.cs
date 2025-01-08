using InventoryManagement.Core.DTO;
using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Product.Queries.GetSingle;

public class GetProductQueryHandler:IRequestHandler<GetProductQuery, Products?>
{
    private readonly IBaseRepository<Entities.Product> _productRepository;

    public GetProductQueryHandler(IBaseRepository<Entities.Product> productRepository)
    {
        _productRepository = productRepository;
    }
    
    public async Task<Products?> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id);
        if(product == null)
            return null;

        return new Products
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            StockQuantity = product.StockQuantity
        };
    }
}
