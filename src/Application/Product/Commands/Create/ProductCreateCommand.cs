namespace Microsoft.Extensions.DependencyInjection.Product.Commands.Create;

public class ProductCreateCommand: IRequest<ProductCreateResponse>
{
    public string? Name { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? Description { get; set; }

    public ProductCreateCommand(string name, Guid categoryId, decimal price, int stockQuantity, string? description)
    {
        Name = name;
        Price = price;
        CategoryId = categoryId;
        StockQuantity = stockQuantity;
        Description = description;
    }
}
