using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Application.Common.Models;
using Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.WarehouseStock.Queries.GetCollection;

public class GetStocksQueryHandler : IRequestHandler<GetStocksQuery, GetsStocksResult>
{
    private readonly IStockRepository _stockRepository;

    public GetStocksQueryHandler(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }
    
    public async Task<GetsStocksResult> Handle(GetStocksQuery request, CancellationToken cancellationToken)
    {
        var query = _stockRepository.Query();
        
        if (request.WarehouseUid.HasValue)
            query = query.Where(s => s.Warehouse.Uid == request.WarehouseUid.Value);

        if (request.ProductUid.HasValue)
            query = query.Where(s => s.Product.Uid == request.ProductUid.Value);
        
        var stocks = await PaginatedList<StockDTO>
            .CreateAsync(query.Select(StockDTO.Projection), request.PageNumber, request.PageSize);
        
        return new GetsStocksResult
        {
            Stocks = stocks.Items.ToList(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount= stocks.TotalCount,
            TotalPages = stocks.TotalPages
        };
        
        
    }
}
