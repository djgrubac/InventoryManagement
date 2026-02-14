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
            ?? throw new NotFoundException("Order", request.OrderUid.ToString());

        if (order.Status == OrderStatus.Completed)
            throw new Exception("Completed order cannot be changed");

        if (request.NewStatus == OrderStatus.Completed)
        {
            if (!order.Items.Any())
                throw new Exception("Cannot complete empty order");

            await ApplyStockChanges(order);
        }

        order.Status = request.NewStatus;
        await _orderRepository.UpdateAsync(order);
    }

    private async Task ApplyStockChanges(Entities.Order order)
    {
        foreach (var item in order.Items)
        {
            var stock = await _stockRepository.GetAsync(
                order.Warehouse.Uid,
                item.Product.Uid
            );

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

