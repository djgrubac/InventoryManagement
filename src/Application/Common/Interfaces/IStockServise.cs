using Inventory_Management.Application.Common.Models;
using Inventory_Management.Domain.Entities;

namespace Inventory_Management.Application.Common.Interfaces;

public interface IStockService
{
    Task<StockDTO?> GetAsync(Guid warehouseUid, Guid productUid);
    Task<IEnumerable<StockDTO>> GetForWarehouseAsync(Guid warehouseUid);
    Task SetQuantityAsync(Guid warehouseUid, Guid productUid, int quantity);
}
