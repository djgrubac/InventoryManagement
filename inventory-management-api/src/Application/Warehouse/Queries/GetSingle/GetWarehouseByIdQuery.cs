using Inventory_Management.Application.Common.Models;

namespace Microsoft.Extensions.DependencyInjection.Warehouse.Queries.GetSingle;

public class GetWarehouseByIdQuery:IRequest<WarehouseDTO>
{
    public Guid Uid { get; set; }

    public GetWarehouseByIdQuery(Guid uid)
    {
        Uid = uid;
    }
}
