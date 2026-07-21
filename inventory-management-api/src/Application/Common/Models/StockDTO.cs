using System.Linq.Expressions;

namespace Inventory_Management.Application.Common.Models;

public class StockDTO
{
    public required WarehouseDTO Warehouse { get; init; }
    public required ProductDTO Product { get; init; }
    public required int Quantity { get; init; }
    public static Expression<Func<Domain.Entities.Stock, StockDTO>> Projection
    {
        get
        {
            return entity => new StockDTO
            {
                Warehouse = new WarehouseDTO
                {
                    Uid = entity.Warehouse.Uid,
                    Name = entity.Warehouse.Name ?? string.Empty,
                    Address = entity.Warehouse.Address,
                    ContactPerson = entity.Warehouse.ContactPerson
                },
                Product = new ProductDTO
                {
                    Uid = entity.Product.Uid,
                    Name = entity.Product.Name ?? string.Empty,
                    Price = entity.Product.Price,
                    StockQuantity = entity.Product.StockQuantity,
                    Description = entity.Product.Description
                },
                Quantity = entity.Quantity
            };
        }
    }
}
