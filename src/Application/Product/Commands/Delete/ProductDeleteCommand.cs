namespace Microsoft.Extensions.DependencyInjection.Product.Commands.Delete;

public class ProductDeleteCommand: IRequest
{
    public Guid Id { get; set; }

    public ProductDeleteCommand(Guid id)
    {
        Id = id;
    }
}
