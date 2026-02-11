using System.Linq.Expressions;
using Inventory_Management.Domain.Entities;

namespace Inventory_Management.Application.Common.Models;

public class OrderItemDTO
{
    public required ProductDTO Product { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public required decimal Total { get; init; }

    public static Expression<Func<OrderItem, OrderItemDTO>> Projection
    {
        get
        {
            return entity => new OrderItemDTO
            {
                Product = new ProductDTO
                {
                    Uid = entity.Product.Uid,
                    Name = entity.Product.Name ?? string.Empty,
                    Price = entity.Product.Price,
                    StockQuantity = entity.Product.StockQuantity,
                    Description = entity.Product.Description
                },
                Quantity = entity.Quantity,
                UnitPrice = entity.Price,
                Total = entity.Price * entity.Quantity
            };
        }
    }
}
