using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Domain.Enums;
using Inventory_Management.Domain.Entities;
using InventoryManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Order;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBaseRepository<Entities.Product> _productRepository;
    private readonly IBaseRepository<Entities.Warehouse> _warehouseRepository;
    private readonly IStockRepository _stockRepository;

    public OrderService
    (
        IOrderRepository orderRepository,
        IBaseRepository<Entities.Product> productRepository,
        IBaseRepository<Entities.Warehouse> warehouseRepository,
        IStockRepository stockRepository
    )
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
        _stockRepository = stockRepository;
    }
    
    public async Task<Guid> CreateOrderAsync(OrderType type, Guid warehouseUid)
    {
        var warehouse = await _warehouseRepository.GetByUidAsync(warehouseUid)
                         ?? throw new Exception("Warehouse not found");

        var order = new Entities.Order
        {
            Uid = Guid.NewGuid(),
            Type = type,
            Status = OrderStatus.Pending,
            Created = DateTime.UtcNow,
            WarehouseId = warehouse.Id
        };

        await _orderRepository.AddAsync(order);
        return order.Uid;
    }

    public async Task AddItemAsync(Guid orderUid, Guid productUid, int quantity)
    {
        if (quantity <= 0)
            throw new Exception("Quantity must be positive");

        var order = await _orderRepository.GetByUidAsync(orderUid)
                    ?? throw new Exception("Order not found");

        if (order.Status != OrderStatus.Pending)
            throw new Exception("You can only add items to pending orders");

        var product = await _productRepository.GetByUidAsync(productUid) ?? throw new Exception("Product not found");

        order.Items.Add(new OrderItem
        {
            ProductId = product.Id,
            Quantity = quantity,
            Price = product.Price
        });

        order.TotalPrice = order.Items.Sum(i => i.Price * i.Quantity);

        await _orderRepository.UpdateAsync(order);
    }

    public async Task RemoveItemAsync(Guid orderUid, Guid productUid)
    {
        var order = await _orderRepository.GetByUidAsync(orderUid)
                    ?? throw new Exception("Order not found");

        if (order.Status != OrderStatus.Pending)
            throw new Exception("You can only remove items from pending orders");

        var product = await _productRepository.GetByUidAsync(productUid)
                      ?? throw new Exception("Product not found");

        var item = order.Items.FirstOrDefault(i => i.ProductId == product.Id);
        if (item == null)
            throw new Exception("Order item not found for the specified product");

        order.Items.Remove(item);
        order.TotalPrice = order.Items.Sum(i => i.Price * i.Quantity);

        await _orderRepository.UpdateAsync(order);
    }

    public async Task ChangeStatusAsync(Guid orderUid, OrderStatus newStatus)
    {
        var order = await _orderRepository.GetByUidAsync(orderUid)
                    ?? throw new Exception("Order not found");

        // Completed or cancelled orders are terminal – no further changes allowed
        if (order.Status is OrderStatus.Completed or OrderStatus.Cancelled)
            throw new Exception("Completed or cancelled order cannot be modified");

        if (newStatus == OrderStatus.Completed)
        {
            if (!order.Items.Any())
                throw new Exception("Cannot complete empty order");

            await ApplyStockChanges(order);
        }

        order.Status = newStatus;
        await _orderRepository.UpdateAsync(order);
    }

    public async Task<Entities.Order?> GetByUidAsync(Guid uid)
    {
        return await _orderRepository.GetByUidAsync(uid);
    }
    
    private async Task ApplyStockChanges(Entities.Order order)
    {
        // Ensure warehouse and products are available and consistent
        if (order.Warehouse == null)
            throw new Exception("Order warehouse is not loaded");

        var warehouseUid = order.Warehouse.Uid;
        var productUids = order.Items
            .Select(i => i.Product?.Uid)
            .Where(uid => uid != null)
            .Cast<Guid>()
            .Distinct()
            .ToList();

        if (!productUids.Any())
            throw new Exception("Order items must have products loaded");
        
        var existingStocks = await _stockRepository.Query()
            .Where(s => s.Warehouse.Uid == warehouseUid && productUids.Contains(s.Product.Uid))
            .ToListAsync();

        var stockByProductUid = existingStocks.ToDictionary(s => s.Product.Uid, s => s);

        foreach (var item in order.Items)
        {
            if (item.Product == null)
                throw new Exception("Order item product is not loaded");

            var productUid = item.Product.Uid;
            stockByProductUid.TryGetValue(productUid, out var stock);

            if (order.Type == OrderType.Incoming)
            {
                if (stock == null)
                {
                    stock = new Stock
                    {
                        WarehouseId = order.WarehouseId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    };

                    await _stockRepository.AddAsync(stock);
                    stockByProductUid[productUid] = stock;
                }
                else
                {
                    stock.Quantity += item.Quantity;
                    await _stockRepository.UpdateAsync(stock);
                }
            }
            else // Outgoing
            {
                if (stock == null)
                    throw new Exception("Stock does not exist");

                if (stock.Quantity < item.Quantity)
                    throw new Exception("Insufficient stock");

                stock.Quantity -= item.Quantity;
                await _stockRepository.UpdateAsync(stock);
            }
        }
    }

}
