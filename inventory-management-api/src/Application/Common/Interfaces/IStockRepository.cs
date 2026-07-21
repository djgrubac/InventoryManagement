using Inventory_Management.Domain.Entities;

namespace Inventory_Management.Application.Common.Interfaces;

public interface IStockRepository
{
    IQueryable<Stock> Query();
    Task<Stock?> GetAsync(Guid warehouseUid, Guid productUid, CancellationToken cancellationToken = default);
    Task <IEnumerable<Stock>> GetByWarehouseAsync(Guid warehouseUid, CancellationToken cancellationToken = default);
    // Task<Product?> GetByProductAsync(Guid productUid, CancellationToken cancellationToken = default);
    Task AddAsync(Stock stock);
    Task UpdateAsync(Stock stock);
}
