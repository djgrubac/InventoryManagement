using MediatR;

namespace Microsoft.Extensions.DependencyInjection.Product.Events.Create;

public class ProductCreateEvent:INotification
{
    public Guid ProductId { get; }
    public string? Name { get; }
    // public Guid ProductCategoryId { get; }
    public string? Description { get; }
    public decimal Price { get; }

    public ProductCreateEvent(Guid productId, string? name, string? description, decimal price)
    {
        ProductId = productId;
        Name = name;
        Description = description;
        Price = price;
    }
}
