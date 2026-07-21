using Inventory_Management.Application.Common.Models;

namespace Microsoft.Extensions.DependencyInjection.WarehouseStock.Queries;

public class GetStockByUidQuery : IRequest<StockDTO?>
{
    public Guid WarehouseUid { get; set; }
    public Guid ProductUid { get; set; }

    public GetStockByUidQuery(Guid warehouseUid, Guid productUid)
    {
        WarehouseUid = warehouseUid;
        ProductUid = productUid;
    }
}
