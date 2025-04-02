using InventoryManagement.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection.Product.Events.Create;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Product.Commands.Create;

public class ProductCreateCommandHandler:IRequestHandler<ProductCreateCommand, Guid>
{
    private readonly IBaseRepository<Entities.Product> _productRepository;
    private readonly IMediator _mediator;
    public ProductCreateCommandHandler(IBaseRepository<Entities.Product> productRepository, IMediator mediator)
    {
        _productRepository = productRepository;
        _mediator = mediator;
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
        
        await _mediator.Publish(new ProductCreateEvent(product.Id, product.Name, product.Description, product.Price), cancellationToken);

        return product.Id;
    }
}
