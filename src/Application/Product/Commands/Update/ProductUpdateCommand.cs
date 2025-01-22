namespace Microsoft.Extensions.DependencyInjection.Product.Commands.Update;

public class ProductUpdateCommand: IRequest
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? Description { get; set; }

    public ProductUpdateCommand(Guid id, string name, decimal price, int stockQuantity, string description)
    {
        Id = id;
        Name = name;
        Price = price;
        StockQuantity = stockQuantity;
        Description = description;
    }
}
