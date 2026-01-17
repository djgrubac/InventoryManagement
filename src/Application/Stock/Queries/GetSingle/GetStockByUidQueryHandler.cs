using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Application.Common.Models;
using Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.WarehouseStock.Queries;

public class GetStockByUidQueryHandler : IRequestHandler<GetStockByUidQuery, StockDTO?>
{
    private readonly IStockRepository _stockRepository;

    public GetStockByUidQueryHandler(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }
    
    public async Task<StockDTO?> Handle(GetStockByUidQuery request, CancellationToken cancellationToken)
    {
        var stock = await _stockRepository.GetAsync(request.WarehouseUid, request.ProductUid);
        
        if (stock == null)
            return null;

        return StockDTO.Projection.Compile().Invoke(stock);
    }
}
