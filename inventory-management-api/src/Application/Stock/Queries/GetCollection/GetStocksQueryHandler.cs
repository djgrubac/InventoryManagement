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

        if (request.CategoryUid.HasValue)
            query = query.Where(s => s.Product.Category.Uid == request.CategoryUid.Value);

        // Paging needs a deterministic order, otherwise rows can repeat or go missing between pages.
        query = query.OrderBy(s => s.Warehouse.Name).ThenBy(s => s.Product.Name).ThenBy(s => s.Id);
        
        var stocks = await PaginatedList<StockDTO>
            .CreateAsync(query.Select(StockDTO.Projection), request.PageNumber, request.PageSize, cancellationToken);
        
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
