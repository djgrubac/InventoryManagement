using Microsoft.Extensions.DependencyInjection.Product.Commands.Create;

namespace Microsoft.Extensions.DependencyInjection.ProductCategory.Commands.Create;

public class CreateCategoryCommand:IRequest<CategoryResponse>
{
    public string? Caption { get; set; }
}
