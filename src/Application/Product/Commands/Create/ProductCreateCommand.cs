namespace Microsoft.Extensions.DependencyInjection.Product.Commands.Create;

public class ProductCreateCommand: IRequest<Guid>
{
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? Description { get; set; }

    public ProductCreateCommand(string name, decimal price, int stockQuantity, string? description)
    {
        Name = name;
        Price = price;
        StockQuantity = stockQuantity;
        Description = description;
    }
}
