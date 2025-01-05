using InventoryManagement.Core.DTO;

namespace Microsoft.Extensions.DependencyInjection.Product.Queries.GetSingle;

public class GetProductQuery:IRequest<Products?>
{
    public Guid Id { get; set; }

    public GetProductQuery(Guid id)
    {
        Id = id;
    }
}
