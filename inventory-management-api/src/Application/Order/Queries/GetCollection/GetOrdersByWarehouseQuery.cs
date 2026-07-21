using Inventory_Management.Application.Common.Models;

namespace Microsoft.Extensions.DependencyInjection.Order.Queries.GetCollection;

public class GetOrdersByWarehouseQuery : IRequest<IEnumerable<OrderDTO>>
{
    public Guid WarehouseUid { get; set; }

    public GetOrdersByWarehouseQuery(Guid warehouseUid)
    {
        WarehouseUid = warehouseUid;
    }
}

