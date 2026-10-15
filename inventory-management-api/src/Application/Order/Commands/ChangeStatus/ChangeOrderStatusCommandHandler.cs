using ValidationException = Inventory_Management.Application.Common.Exceptions.ValidationException;
using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Domain.Entities;
using Inventory_Management.Domain.Enums;
using Entities = Inventory_Management.Domain.Entities;


namespace Microsoft.Extensions.DependencyInjection.Order.Commands.ChangeStatus;

public class ChangeOrderStatusCommandHandler : IRequestHandler<ChangeOrderStatusCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IStockRepository _stockRepository;

    public ChangeOrderStatusCommandHandler(
        IOrderRepository orderRepository,
        IStockRepository stockRepository)
    {
        _orderRepository = orderRepository;
        _stockRepository = stockRepository;
    }

    public async Task Handle(ChangeOrderStatusCommand request, CancellationToken ct)
    {
        var order = await _orderRepository.GetByUidAsync(request.OrderUid)
            ?? throw new NotFoundException(request.OrderUid.ToString(), "Order");

        // Completed and cancelled orders are terminal
        if (order.Status is OrderStatus.Completed or OrderStatus.Cancelled)
            throw new ValidationException(nameof(request.NewStatus), $"A {order.Status} order cannot be changed.");

        if (request.NewStatus == OrderStatus.Completed)
        {
            if (!order.Items.Any())
                throw new ValidationException(nameof(request.NewStatus), "Cannot complete an empty order.");

            await ApplyStockChanges(order, ct);
        }

        order.Status = request.NewStatus;
        await _orderRepository.UpdateAsync(order);
    }

    private async Task ApplyStockChanges(Entities.Order order, CancellationToken ct)
    {
        var stocks = new Dictionary<int, Stock?>();
        foreach (var item in order.Items)
        {
            stocks[item.ProductId] = await _stockRepository.GetAsync(order.Warehouse.Uid, item.Product.Uid, ct);
        }

        // Check every line before touching stock, so a failing line can't leave
        // earlier lines already deducted.
        if (order.Type == OrderType.Outgoing)
        {
            foreach (var item in order.Items)
            {
                var available = stocks[item.ProductId]?.Quantity ?? 0;
                if (available < item.Quantity)
                    throw new ValidationException(nameof(order.Items),
                        $"Insufficient stock for {item.Product.Name}: {available} available, {item.Quantity} required.");
            }
        }

        foreach (var item in order.Items)
        {
            var stock = stocks[item.ProductId];

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
                }
                else
                {
                    stock.Quantity += item.Quantity;
                    await _stockRepository.UpdateAsync(stock);
                }
            }
            else // Outgoing
            {
                stock!.Quantity -= item.Quantity;
                await _stockRepository.UpdateAsync(stock);
            }
        }
    }
}
