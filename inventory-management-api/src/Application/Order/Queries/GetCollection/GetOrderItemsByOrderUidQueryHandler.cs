using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Application.Common.Models;

namespace Microsoft.Extensions.DependencyInjection.Order.Queries.GetCollection;

public class GetOrderItemsByOrderUidQueryHandler : IRequestHandler<GetOrderItemsByOrderUidQuery, IEnumerable<OrderItemDTO>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderItemsByOrderUidQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<OrderItemDTO>> Handle(GetOrderItemsByOrderUidQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByUidAsync(request.OrderUid);
        if (order == null)
            return Enumerable.Empty<OrderItemDTO>();

        var projector = OrderItemDTO.Projection.Compile();
        return order.Items.Select(projector);
    }
}

