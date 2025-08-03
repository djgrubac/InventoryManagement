using Inventory_Management.Application.Common.Interfaces;
using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Product.Commands.Create;

public class ProductCreateCommandHandler:IRequestHandler<ProductCreateCommand, Guid>
{
    private readonly IBaseRepository<Entities.Product> _productRepository;
    private readonly IProductCategoryRepository _productCategoryRepository;
    private readonly IMediator _mediator;
    public ProductCreateCommandHandler(IBaseRepository<Entities.Product> productRepository, IMediator mediator, IProductCategoryRepository productCategoryRepository)
    {
        _productRepository = productRepository;
        _mediator = mediator;
        _productCategoryRepository = productCategoryRepository;
    }
    public async Task<Guid> Handle(ProductCreateCommand request, CancellationToken cancellationToken)
    {
        var category = await _productCategoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null)
        {
            throw new Exception($"Product category with ID {request.CategoryId} does not exist.");
        }
        
        var product = new Entities.Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            Description = request.Description,
            ProductCategoryId = request.CategoryId,
            CreatedAt = DateTime.UtcNow
        };
        await _productRepository.AddAsync(product);

        return product.Id;
    }
}
