using Inventory_Management.Domain.Entities;

namespace Inventory_Management.Application.Common.Interfaces;

public interface IOrderItemRepository
{
    Task AddAsync(OrderItem item);
    Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId);
}
