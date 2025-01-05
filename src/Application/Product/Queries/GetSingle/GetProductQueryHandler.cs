using InventoryManagement.Core.DTO;
using InventoryManagement.Core.Interfaces;

namespace Microsoft.Extensions.DependencyInjection.Product.Queries.GetSingle;

public class GetProductQueryHandler:IRequestHandler<GetProductQuery, Products?>
{
    private readonly IBaseRepository<Inventory_Management.Domain.Entities.Product> _productRepository;

    public GetProductQueryHandler(IBaseRepository<Inventory_Management.Domain.Entities.Product> productRepository)
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
