using Inventory_Management.Application.Common.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.ProductCategory.Commands.Create;

public class CreateCategoryCommandHandler:IRequestHandler<CreateCategoryCommand, CategoryResponse>
{
    private readonly ICategoryRepository _repository;

    public CreateCategoryCommandHandler(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<CategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Entities.Category
        {
            Caption = request.Caption
        };
        
        await _repository.AddAsync(category);

        return new CategoryResponse { Id = category.Id, Caption = category.Caption, };
    }
}
