using Inventory_Management.Application.Common.Interfaces;

namespace Microsoft.Extensions.DependencyInjection.Order.Commands.RemoveItem;

public class RemoveOrderItemCommandHandler : IRequestHandler<RemoveOrderItemCommand>
{
    private readonly IOrderService _orderService;

    public RemoveOrderItemCommandHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task Handle(RemoveOrderItemCommand request, CancellationToken cancellationToken)
    {
        await _orderService.RemoveItemAsync(request.OrderUid, request.ProductUid);
    }
}

