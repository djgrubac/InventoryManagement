using System.Linq.Expressions;


namespace Inventory_Management.Application.Common.Models;

public class ProductDTO
{
    public Guid Uid { get; set; }
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public int StockQuantity { get; set; }
    public string? Description { get; set; }
    
    public static Expression<Func<Domain.Entities.Product, ProductDTO>> Projection
    {
        get
        {
            return entity => new ProductDTO()
            {
                Uid = entity.Uid,
                Name = entity.Name,
                Price = entity.Price,
                StockQuantity = entity.StockQuantity,
                Description = entity.Description
            };
        }
    }
}
