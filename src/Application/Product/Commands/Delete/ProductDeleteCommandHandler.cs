using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Product.Commands.Delete;

public class ProductDeleteCommandHandler:IRequestHandler<ProductDeleteCommand>
{
    private readonly IBaseRepository<Entities.Product> _productRepository;

    public ProductDeleteCommandHandler(IBaseRepository<Entities.Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task Handle(ProductDeleteCommand request, CancellationToken cancellationToken)
    {
        var product = _productRepository.GetByIdAsync(request.Id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {request.Id} not found.");
        }
        
        await _productRepository.DeleteAsync(request.Id);
    }
}
