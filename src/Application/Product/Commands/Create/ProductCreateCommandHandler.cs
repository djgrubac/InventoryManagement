using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Product.Commands.Create;

public class ProductCreateCommandHandler:IRequestHandler<ProductCreateCommand, Guid>
{
    private readonly IBaseRepository<Entities.Product> _productRepository;
    public ProductCreateCommandHandler(IBaseRepository<Entities.Product> productRepository)
    {
        _productRepository = productRepository;
    }
    public async Task<Guid> Handle(ProductCreateCommand request, CancellationToken cancellationToken)
    {
        var product = new Entities.Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };
        await _productRepository.AddAsync(product);
        return product.Id;
    }
}
