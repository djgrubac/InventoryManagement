using Inventory_Management.Application.Common.Models;
using Inventory_Management.Application.Common.Interfaces;

namespace Microsoft.Extensions.DependencyInjection.Order.Queries.GetCollection;

public class GetOrdersByWarehouseQueryHandler : IRequestHandler<GetOrdersByWarehouseQuery, IEnumerable<OrderDTO>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersByWarehouseQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<OrderDTO>> Handle(GetOrdersByWarehouseQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByWarehouseUidAsync(request.WarehouseUid);

        var projector = OrderDTO.Projection.Compile();
        return orders.Select(projector);
    }
}

