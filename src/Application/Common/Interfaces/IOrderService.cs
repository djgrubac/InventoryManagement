using Inventory_Management.Domain.Entities;
using Inventory_Management.Domain.Enums;

namespace Inventory_Management.Application.Common.Interfaces;

public interface IOrderService
{
    Task<Guid> CreateOrderAsync(OrderType type, Guid warehouseUid);
    Task AddItemAsync(Guid orderUid, Guid productUid, int quantity);
    Task RemoveItemAsync(Guid orderUid, Guid productUid);
    Task ChangeStatusAsync(Guid orderUid, OrderStatus newStatus);
    Task<Order?> GetByUidAsync(Guid uid);
}
