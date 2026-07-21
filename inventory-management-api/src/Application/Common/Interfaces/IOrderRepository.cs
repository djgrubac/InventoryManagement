using Inventory_Management.Domain.Entities;

namespace Inventory_Management.Application.Common.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByUidAsync(Guid uid);
    Task<Order?> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<IEnumerable<Order>> GetByWarehouseUidAsync(Guid warehouseUid);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
}
