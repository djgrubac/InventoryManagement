using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Domain.Enums;

namespace Microsoft.Extensions.DependencyInjection.Order.Commands.Create;

public class OrderCreateCommandHandler : IRequestHandler<OrderCreateCommand, Guid>
{
    private readonly IOrderService _orderService;

    public OrderCreateCommandHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Guid> Handle(OrderCreateCommand request, CancellationToken cancellationToken)
    {
        return await _orderService.CreateOrderAsync(request.Type, request.WarehouseUid);
    }
}

