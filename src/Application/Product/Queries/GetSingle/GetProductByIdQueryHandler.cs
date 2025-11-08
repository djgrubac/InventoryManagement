using InventoryManagement.Core.DTO;
using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Product.Queries.GetSingle;

public class GetProductByIdQueryHandler:IRequestHandler<GetProductByIdQuery, ProductDTO?>
{
    private readonly IBaseRepository<Entities.Product> _productRepository;

    public GetProductByIdQueryHandler(IBaseRepository<Entities.Product> productRepository)
    {
        _productRepository = productRepository;
    }
    
    public async Task<ProductDTO?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id);
        if(product == null)
            return null;

        return new ProductDTO
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Description = product.Description
        };
    }
}
