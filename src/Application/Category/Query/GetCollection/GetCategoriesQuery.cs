using Models = Inventory_Management.Application.Common.Models;

namespace Microsoft.Extensions.DependencyInjection.ProductCategory.Query.GetCollection;

public class GetCategoriesQuery:IRequest<IEnumerable<Models.CategoryDTO>> { }
