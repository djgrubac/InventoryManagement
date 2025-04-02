namespace Microsoft.Extensions.DependencyInjection.ProductCategory.Commands.Create;

public class ProductCategoryCreateCommand:IRequest<Guid>
{
    public Guid Id { get; set; }
    public string? Caption { get; set; }
}
