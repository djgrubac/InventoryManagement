namespace Microsoft.Extensions.DependencyInjection.WarehouseStock.Queries.GetCollection;

public class GetStocksQuery : IRequest<GetsStocksResult>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public Guid? WarehouseUid { get; set; }
    public Guid? ProductUid { get; set; }
}
