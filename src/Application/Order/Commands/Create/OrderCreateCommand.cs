using Inventory_Management.Domain.Enums;

namespace Microsoft.Extensions.DependencyInjection.Order.Commands.Create;

public class OrderCreateCommand : IRequest<Guid>
{
    public OrderType Type { get; set; }
    public Guid WarehouseUid { get; set; }

    public OrderCreateCommand(OrderType type, Guid warehouseUid)
    {
        Type = type;
        WarehouseUid = warehouseUid;
    }
}

