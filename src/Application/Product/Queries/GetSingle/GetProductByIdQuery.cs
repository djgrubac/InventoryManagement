using InventoryManagement.Core.DTO;

namespace Microsoft.Extensions.DependencyInjection.Product.Queries.GetSingle;

public class GetProductByIdQuery:IRequest<ProductDTO?>
{
    public Guid Id { get; set; }

    public GetProductByIdQuery(Guid id)
    {
        Id = id;
    }
}
