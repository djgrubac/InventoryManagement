using Inventory_Management.Application.Common.Models;

namespace Microsoft.Extensions.DependencyInjection.Order.Queries.GetCollection;

public class GetOrderItemsByOrderUidQuery : IRequest<IEnumerable<OrderItemDTO>>
{
    public Guid OrderUid { get; set; }

    public GetOrderItemsByOrderUidQuery(Guid orderUid)
    {
        OrderUid = orderUid;
    }
}

