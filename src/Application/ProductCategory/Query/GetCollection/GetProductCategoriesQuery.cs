using Models = Inventory_Management.Application.Common.Models;

namespace Microsoft.Extensions.DependencyInjection.ProductCategory.Query.GetCollection;

public class GetProductCategoriesQuery:IRequest<IEnumerable<Models.ProductCategory>> { }
