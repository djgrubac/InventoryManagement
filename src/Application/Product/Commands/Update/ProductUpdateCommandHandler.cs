using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Product.Commands.Update;

public class ProductUpdateCommandHandler: IRequestHandler<ProductUpdateCommand>
{
    private readonly IBaseRepository<Entities.Product> _productRepository;

    public ProductUpdateCommandHandler(IBaseRepository<Entities.Product> productRepository)
    {
        _productRepository = productRepository;
    }
    
    public async Task Handle(ProductUpdateCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {request.Id} not found.");
        }
        
        product.Name = request.Name;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.Description = request.Description;
        
        await _productRepository.UpdateAsync(product);
    }
}
