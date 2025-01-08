using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Product.Commands.Create;

public class ProductCreateCommandHandler:IRequestHandler<ProductCreateCommand, Guid>
{
    private readonly IBaseRepository<Entities.Product> _producRepository;
    public ProductCreateCommandHandler(IBaseRepository<Entities.Product> producRepository)
    {
        _producRepository = producRepository;
    }
    public async Task<Guid> Handle(ProductCreateCommand request, CancellationToken cancellationToken)
    {
        var product = new Entities.Product
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
