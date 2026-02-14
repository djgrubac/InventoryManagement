using System.Linq.Expressions;
using Inventory_Management.Domain.Entities;
using Inventory_Management.Domain.Enums;

namespace Inventory_Management.Application.Common.Models;

public class OrderDTO
{
    public required Guid Uid { get; init; }
    public required OrderStatus Status { get; init; }
    public required OrderType Type { get; init; }
    public required WarehouseDTO Warehouse { get; init; }
    public required decimal TotalPrice { get; init; }
    public required List<OrderItemDTO> Items { get; init; }
    public required DateTime Created { get; init; }
    
    public static Expression<Func<Order, OrderDTO>> Projection
    {
        get
        {
            return entity => new OrderDTO
            {
                Uid = entity.Uid,
                Status = entity.Status,
                Type = entity.Type,
                Warehouse = new WarehouseDTO
                {
                    Uid = entity.Warehouse.Uid,
                    Name = entity.Warehouse.Name ?? string.Empty,
                    Address = entity.Warehouse.Address,
                    ContactPerson = entity.Warehouse.ContactPerson
                },
                TotalPrice = entity.TotalPrice,
                Items = entity.Items.Select(i => new OrderItemDTO
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
                Created = entity.Created.DateTime
            };
        }
    }
}
