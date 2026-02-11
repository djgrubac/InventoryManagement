using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Application.Common.Models;

namespace Microsoft.Extensions.DependencyInjection.Order.Queries.GetSingle;

public class GetOrderByUidQueryHandler : IRequestHandler<GetOrderByUidQuery, OrderDTO?>
{
    private readonly IOrderService _orderService;

    public GetOrderByUidQueryHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<OrderDTO?> Handle(GetOrderByUidQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByUidAsync(request.Uid);
        if (order == null)
            return null;

        // Use the existing projection shape for consistency
        return new OrderDTO
        {
            Uid = order.Uid,
            Status = order.Status,
            Type = order.Type,
            Warehouse = new WarehouseDTO
            {
                Uid = order.Warehouse.Uid,
                Name = order.Warehouse.Name ?? string.Empty,
                Address = order.Warehouse.Address,
                ContactPerson = order.Warehouse.ContactPerson
            },
            TotalPrice = order.TotalPrice,
            Items = order.Items.Select(i => new OrderItemDTO
            {
                Product = new ProductDTO
                {
                    Uid = i.Product.Uid,
                    Name = i.Product.Name ?? string.Empty,
                    Price = i.Product.Price,
                    StockQuantity = i.Product.StockQuantity,
                    Description = i.Product.Description
                },
                Quantity = i.Quantity,
                UnitPrice = i.Price,
                Total = i.Price * i.Quantity
            }).ToList(),
            Created = order.Created.DateTime
        };
    }
}

