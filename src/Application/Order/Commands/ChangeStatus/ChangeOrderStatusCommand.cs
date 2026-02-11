using Inventory_Management.Domain.Enums;

namespace Microsoft.Extensions.DependencyInjection.Order.Commands.ChangeStatus;

public class ChangeOrderStatusCommand : IRequest
{
    public Guid OrderUid { get; set; }
    public OrderStatus NewStatus { get; set; }

    public ChangeOrderStatusCommand(Guid orderUid, OrderStatus newStatus)
    {
        OrderUid = orderUid;
        NewStatus = newStatus;
    }
}

