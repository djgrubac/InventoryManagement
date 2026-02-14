using Inventory_Management.Application.Common.Models;

namespace Microsoft.Extensions.DependencyInjection.Order.Queries.GetSingle;

public class GetOrderByUidQuery : IRequest<OrderDTO?>
{
    public Guid Uid { get; set; }

    public GetOrderByUidQuery(Guid uid)
    {
        Uid = uid;
    }
}

