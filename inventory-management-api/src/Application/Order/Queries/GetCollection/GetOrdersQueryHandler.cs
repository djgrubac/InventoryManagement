using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Application.Common.Models;

namespace Microsoft.Extensions.DependencyInjection.Order.Queries.GetCollection;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, GetOrdersResult>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<GetOrdersResult> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _orderRepository.Query();

        if (request.WarehouseUid.HasValue)
            query = query.Where(o => o.Warehouse.Uid == request.WarehouseUid.Value);

        if (request.Type.HasValue)
            query = query.Where(o => o.Type == request.Type.Value);

        if (request.Status.HasValue)
            query = query.Where(o => o.Status == request.Status.Value);

        // Newest first; Id breaks ties so paging is stable.
        query = query.OrderByDescending(o => o.Created).ThenByDescending(o => o.Id);

        var orders = await PaginatedList<OrderDTO>
            .CreateAsync(query.Select(OrderDTO.Projection), request.PageNumber, request.PageSize, cancellationToken);

        return new GetOrdersResult
        {
            Orders = orders.Items.ToList(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = orders.TotalCount,
            TotalPages = orders.TotalPages
        };
    }
}
