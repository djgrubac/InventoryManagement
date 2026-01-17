namespace Microsoft.Extensions.DependencyInjection.Product.Commands.Delete;

public class ProductDeleteCommand: IRequest
{
    public Guid Uid { get; set; }

    public ProductDeleteCommand(Guid uid)
    {
        Uid = uid;
    }
}
