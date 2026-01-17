using Inventory_Management.Application.Common.Interfaces;
using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;
using Models = Inventory_Management.Application.Common.Models;

namespace Microsoft.Extensions.DependencyInjection.ProductCategory;

public class CategoryService:ICategoryService
{
    private readonly IBaseRepository<Entities.Category> _productCategoryRepository;

    public CategoryService(IBaseRepository<Entities.Category> productCategoryRepository)
    {
        _productCategoryRepository = productCategoryRepository;
    }
    public async Task<IEnumerable<Models.CategoryDTO>> GetAllAsync()
    {
        var productCategories = await _productCategoryRepository.GetAllAsync();
        return productCategories.Select(pc => new Models.CategoryDTO
        {
            Uid = pc.Uid,
            Caption = pc.Caption
        });
    }

    public async Task<Guid> CreateCategoryAsync(Guid uid, string caption)
    {
        var product = new Entities.Category
        {
            Uid = new Guid(),
            Caption = caption
        };
            await _productCategoryRepository.AddAsync(product);
            return product.Uid;
    }
}
