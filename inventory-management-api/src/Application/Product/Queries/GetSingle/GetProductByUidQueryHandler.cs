using Inventory_Management.Application.Common.Models;
using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Product.Queries.GetSingle;

public class GetProductByUidQueryHandler:IRequestHandler<GetProductByUidQuery, ProductDTO?>
{
    private readonly IBaseRepository<Entities.Product> _productRepository;

    public GetProductByUidQueryHandler(IBaseRepository<Entities.Product> productRepository)
    {
        _productRepository = productRepository;
    }
    
    public async Task<ProductDTO?> Handle(GetProductByUidQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByUidAsync(request.Uid);
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
}
