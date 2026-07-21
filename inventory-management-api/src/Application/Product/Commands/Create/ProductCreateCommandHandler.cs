using Inventory_Management.Application.Common.Interfaces;
using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Product.Commands.Create;

public class ProductCreateCommandHandler:IRequestHandler<ProductCreateCommand, ProductCreateResponse>
{
    private readonly IBaseRepository<Entities.Product> _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMediator _mediator;
    public ProductCreateCommandHandler(IBaseRepository<Entities.Product> productRepository, IMediator mediator, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _mediator = mediator;
        _categoryRepository = categoryRepository;
    }
    public async Task<ProductCreateResponse> Handle(ProductCreateCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByUidAsync(request.CategoryUid);
        if (category == null)
        {
            throw new Exception($"Product category with Uid {request.CategoryUid} does not exist.");
        }
        
        var product = new Entities.Product
        {
            Name = request.Name,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            Description = request.Description,
            CategoryId = category.Id
        };
        await _productRepository.AddAsync(product);

        return new ProductCreateResponse { Uid = product.Uid, Name = product.Name, };
    }
}
