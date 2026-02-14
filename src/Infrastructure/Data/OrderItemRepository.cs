using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management.Infrastructure.Data;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly ApplicationDbContext _context;

    public OrderItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task AddAsync(OrderItem item)
    {
        await _context.OrderItems.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId)
    {
        return await _context.OrderItems
            .Where(i => i.OrderId == orderId)
            .ToListAsync();
    }
}
