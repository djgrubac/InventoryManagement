using Inventory_Management.Application.Common.Interfaces;
using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;
using Models = Inventory_Management.Application.Common.Models;

namespace Microsoft.Extensions.DependencyInjection.ProductCategory;

public class ProductCategoryService:IProductCategoryService
{
    private readonly IBaseRepository<Entities.ProductCategory> _productCategoryRepository;

    public ProductCategoryService(IBaseRepository<Entities.ProductCategory> productCategoryRepository)
    {
        _productCategoryRepository = productCategoryRepository;
    }
    public async Task<IEnumerable<Models.ProductCategory>> GetAllAsync()
    {
        var productCategories = await _productCategoryRepository.GetAllAsync();
        return productCategories.Select(pc => new Models.ProductCategory
        {
            Caption = pc.Caption
        });
    }
}
