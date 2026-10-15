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
    public async Task<Warehouse?> GetByUidAsync(Guid uid)
    {
        return await _context.Warehouses.FirstOrDefaultAsync(w => w.Uid == uid);
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync()
    {
        return await _context.Warehouses.ToListAsync();
    }

    public async Task AddAsync(Warehouse entity)
    {
        await _context.Warehouses.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Warehouse entity)
    {
        _context.Warehouses.Update(entity);
        await _context.SaveChangesAsync();
    }

    public Task DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}
