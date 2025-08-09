using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Domain.Entities;
using InventoryManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management.Infrastructure.Data;

public class CategoryRepository: ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.ProductCategories.ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(Guid id)
    {
        return await _context.ProductCategories.FindAsync(id);
    }

    public async Task AddAsync(Category entity)
    {
        await _context.ProductCategories.AddAsync(entity);
        await _context.SaveChangesAsync();
    }
}
