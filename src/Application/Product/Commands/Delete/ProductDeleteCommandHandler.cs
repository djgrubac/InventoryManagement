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
        var product = _productRepository.GetByUidAsync(request.Uid);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with Uid {request.Uid} not found.");
        }
        
        await _productRepository.DeleteAsync(request.Uid);
    }
}
