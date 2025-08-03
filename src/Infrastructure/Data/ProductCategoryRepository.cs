using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Domain.Entities;
using InventoryManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management.Infrastructure.Data;

public class ProductCategoryRepository: IProductCategoryRepository
{
    private readonly ApplicationDbContext _context;

    public ProductCategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<ProductCategory>> GetAllAsync()
    {
        return await _context.ProductCategories.ToListAsync();
    }

    public async Task<ProductCategory?> GetByIdAsync(Guid id)
    {
        return await _context.ProductCategories.FindAsync(id);
    }

    public async Task AddAsync(ProductCategory entity)
    {
        await _context.ProductCategories.AddAsync(entity);
        await _context.SaveChangesAsync();
    }
}
