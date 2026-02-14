using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Domain.Enums;

namespace Microsoft.Extensions.DependencyInjection.Order.Commands.RemoveItem;

public class RemoveOrderItemCommandHandler : IRequestHandler<RemoveOrderItemCommand>
{
    private readonly IOrderRepository _orderRepository;

    public RemoveOrderItemCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task Handle(RemoveOrderItemCommand request, CancellationToken cancellationToken)
    {
        // Validate order exists
        var order = await _orderRepository.GetByUidAsync(request.OrderUid)
                    ?? throw new NotFoundException("Order", request.OrderUid.ToString());

        // Prevent modification of completed or cancelled orders
        if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException($"Cannot remove items from {order.Status} order");

        // Find the item to remove
        var itemToRemove = order.Items.FirstOrDefault(i => i.Product.Uid == request.ProductUid);
        
        if (itemToRemove == null)
            throw new NotFoundException("OrderItem", request.ProductUid.ToString());

        // Remove the item
        order.Items.Remove(itemToRemove);

        // Recalculate total price
        order.TotalPrice = order.Items.Sum(i => i.Price * i.Quantity);

        await _orderRepository.UpdateAsync(order);
    }
}

