using Inventory_Management.Application.Common.Interfaces;
using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;
using Models = Inventory_Management.Application.Common.Models;

namespace Microsoft.Extensions.DependencyInjection.ProductCategory.Query.GetCollection;

public class GetProductCategoriesQueryHandler:IRequestHandler<GetProductCategoriesQuery, IEnumerable<Models.ProductCategory>>
{
    private readonly IProductCategoryRepository _productCategoryRepository;

    public GetProductCategoriesQueryHandler(IProductCategoryRepository productCategoryRepository)
    {
        _productCategoryRepository = productCategoryRepository;
    }
    
    public async Task<IEnumerable<Models.ProductCategory>> Handle(GetProductCategoriesQuery request, CancellationToken cancellationToken)
    {
        var productCategories = await _productCategoryRepository.GetAllAsync();
    
        return productCategories.Select(pc => new Models.ProductCategory
        {
            Caption = pc.Caption
        });
    }
}
