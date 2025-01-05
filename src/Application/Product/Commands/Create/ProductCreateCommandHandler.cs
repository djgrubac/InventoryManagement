using InventoryManagement.Core.Interfaces;

namespace Microsoft.Extensions.DependencyInjection.Product.Commands.Create;

public class ProductCreateCommandHandler:IRequestHandler<ProductCreateCommand, Guid>
{
    private readonly IBaseRepository<Inventory_Management.Domain.Entities.Product> _producRepository;
    public ProductCreateCommandHandler(IBaseRepository<Inventory_Management.Domain.Entities.Product> producRepository)
    {
        _producRepository = producRepository;
    }
    public async Task<Guid> Handle(ProductCreateCommand request, CancellationToken cancellationToken)
    {
        var product = new Inventory_Management.Domain.Entities.Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CreatedAt = DateTime.UtcNow
        };
        await _producRepository.AddAsync(product);
        return product.Id;
    }
}
