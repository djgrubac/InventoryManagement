using Inventory_Management.Application.Common.Interfaces;
using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;
using Models = Inventory_Management.Application.Common.Models;

namespace Microsoft.Extensions.DependencyInjection.ProductCategory.Query.GetCollection;

public class GetCategoriesQueryHandler:IRequestHandler<GetCategoriesQuery, IEnumerable<Models.ProductCategory>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }
    
    public async Task<IEnumerable<Models.ProductCategory>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var productCategories = await _categoryRepository.GetAllAsync();
    
        return productCategories.Select(pc => new Models.ProductCategory
        {
            Caption = pc.Caption
        });
    }
}
