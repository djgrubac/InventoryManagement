using Inventory_Management.Application.Common.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.ProductCategory.Commands.Create;

public class ProductCategoryCreateCommandHandler:IRequestHandler<ProductCategoryCreateCommand, Guid>
{
    private readonly IProductCategoryRepository _repository;

    public ProductCategoryCreateCommandHandler(IProductCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(ProductCategoryCreateCommand request, CancellationToken cancellationToken)
    {
        var category = new Entities.ProductCategory
        {
            Id = request.Id, 
            Caption = request.Caption
        };
        
        await _repository.AddAsync(category);
        
        return category.Id;
    }
}
