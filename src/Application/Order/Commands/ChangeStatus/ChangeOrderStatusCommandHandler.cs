using Inventory_Management.Application.Common.Interfaces;

namespace Microsoft.Extensions.DependencyInjection.Order.Commands.ChangeStatus;

public class ChangeOrderStatusCommandHandler : IRequestHandler<ChangeOrderStatusCommand>
{
    private readonly IOrderService _orderService;

    public ChangeOrderStatusCommandHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task Handle(ChangeOrderStatusCommand request, CancellationToken cancellationToken)
    {
        await _orderService.ChangeStatusAsync(request.OrderUid, request.NewStatus);
    }
}

