using Inventory_Management.Domain.Entities;
using InventoryManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management.Infrastructure.Data;

public class WarehouseRepository:IBaseRepository<Warehouse>
{
    private readonly ApplicationDbContext _context;

    public WarehouseRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Warehouse?> GetByIdAsync(Guid id)
    {
        return await _context.Wearhouses.FindAsync(id);
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync()
    {
        return await _context.Wearhouses.ToListAsync();
    }

    public async Task AddAsync(Warehouse entity)
    {
        await _context.Wearhouses.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Warehouse entity)
    {
        _context.Wearhouses.Update(entity);
        await _context.SaveChangesAsync();
    }

    public Task DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}
