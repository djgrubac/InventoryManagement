using Inventory_Management.Domain.Enums;

namespace Microsoft.Extensions.DependencyInjection.Order.Queries.GetCollection;

public class GetOrdersQuery : IRequest<GetOrdersResult>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid? WarehouseUid { get; set; }
    public OrderType? Type { get; set; }
    public OrderStatus? Status { get; set; }
}
