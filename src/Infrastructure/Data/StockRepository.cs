using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Domain.Entities;
using Inventory_Management.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management.Infrastructure.Repositories;

public class StockRepository : IStockRepository
{
    private readonly ApplicationDbContext _context;

    public StockRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public IQueryable<Stock> Query()
    {
        return _context.Stocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse);
    }

    public async Task<Stock?> GetAsync(Guid warehouseUid, Guid productUid, CancellationToken cancellationToken = default)
    {
        return await _context.Stocks
            .Include(s => s.Warehouse)
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s =>
                    s.Warehouse.Uid == warehouseUid &&
                    s.Product.Uid == productUid,
                cancellationToken);
    }

    public async Task<IEnumerable<Stock>> GetByWarehouseAsync(Guid warehouseUid, CancellationToken cancellationToken = default)
    {
        return await _context.Stocks
            .Include(s => s.Warehouse)
            .Include(s => s.Product)
            .Where(s => s.Warehouse.Uid == warehouseUid)
            .ToListAsync(cancellationToken);
    }

    // public async Task<Product?> GetByProductAsync(Guid productUid, CancellationToken cancellationToken = default)
    // {
    //     return await _context.Products
    //         .FirstOrDefaultAsync(p => p.Uid == productUid, cancellationToken);
    // }

    public async Task AddAsync(Stock stock)
    {
        await _context.Stocks.AddAsync(stock);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Stock stock)
    {
        _context.Stocks.Update(stock);
        await _context.SaveChangesAsync();
    }
}
