using Inventory_Management.Application.Common.Models;

namespace Microsoft.Extensions.DependencyInjection.Order.Queries.GetCollection;

public class GetOrdersResult
{
    public List<OrderDTO>? Orders { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}
