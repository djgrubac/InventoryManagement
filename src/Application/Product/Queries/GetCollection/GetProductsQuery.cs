using Inventory_Management.Application.Common.Models;

namespace Microsoft.Extensions.DependencyInjection.Product.Queries.GetCollection;

public class GetProductsQuery: IRequest<IEnumerable<ProductDTO>> {}
