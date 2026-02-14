using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Application.Common.Models;

namespace Microsoft.Extensions.DependencyInjection.Order.Queries.GetSingle;

public class GetOrderByUidQueryHandler : IRequestHandler<GetOrderByUidQuery, OrderDTO?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByUidQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDTO?> Handle(GetOrderByUidQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByUidAsync(request.Uid);
        if (order == null)
            return null;

        var projector = OrderDTO.Projection.Compile();
        return projector(order);
    }
}

