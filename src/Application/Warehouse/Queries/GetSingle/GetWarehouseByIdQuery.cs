using Inventory_Management.Application.Common.Models;

namespace Microsoft.Extensions.DependencyInjection.Warehouse.Queries.GetSingle;

public class GetWarehouseByIdQuery:IRequest<WarehouseDTO>
{
    public Guid Id { get; set; }

    public GetWarehouseByIdQuery(Guid id)
    {
        Id = id;
    }
}
