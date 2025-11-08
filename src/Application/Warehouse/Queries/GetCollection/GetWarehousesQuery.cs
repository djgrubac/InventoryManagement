using Inventory_Management.Application.Common.Models;

namespace Microsoft.Extensions.DependencyInjection.Warehouse.Queries;

public class GetWarehousesQuery: IRequest<IEnumerable<WarehouseDTO>> {}
