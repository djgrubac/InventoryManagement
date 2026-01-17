using Inventory_Management.Application.Common.Models;

namespace Microsoft.Extensions.DependencyInjection.Product.Queries.GetSingle;

public class GetProductByUidQuery:IRequest<ProductDTO?>
{
    public Guid Uid { get; set; }

    public GetProductByUidQuery(Guid uid)
    {
        Uid = uid;
    }
}
