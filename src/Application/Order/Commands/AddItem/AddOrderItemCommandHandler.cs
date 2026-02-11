using Inventory_Management.Application.Common.Interfaces;

namespace Microsoft.Extensions.DependencyInjection.Order.Commands.AddItem;

public class AddOrderItemCommandHandler : IRequestHandler<AddOrderItemCommand>
{
    private readonly IOrderService _orderService;

    public AddOrderItemCommandHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
    {
        await _orderService.AddItemAsync(request.OrderUid, request.ProductUid, request.Quantity);
    }
}

