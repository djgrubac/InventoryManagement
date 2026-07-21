namespace Microsoft.Extensions.DependencyInjection.WarehouseStock.Queries.GetCollection;

public class GetStocksQuery : IRequest<GetsStocksResult>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid? WarehouseUid { get; set; }
    public Guid? ProductUid { get; set; }
}
