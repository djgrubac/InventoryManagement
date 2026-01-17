namespace Microsoft.Extensions.DependencyInjection.Product.Commands.Create;

public class ProductCreateCommand: IRequest<ProductCreateResponse>
{
    public string? Name { get; set; }
    public Guid CategoryUid { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? Description { get; set; }

    public ProductCreateCommand(string name, Guid categoryUid, decimal price, int stockQuantity, string? description)
    {
        Name = name;
        Price = price;
        CategoryUid = categoryUid;
        StockQuantity = stockQuantity;
        Description = description;
    }
}
