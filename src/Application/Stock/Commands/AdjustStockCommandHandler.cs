using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Domain.Entities;
using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.WarehouseStock.Commands;

public class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand>
{
    private readonly IStockRepository _stockRepository;
    private readonly IBaseRepository<Entities.Warehouse> _warehouseRepository;
    private readonly IBaseRepository<Entities.Product> _productRepository;

    public AdjustStockCommandHandler
    (
        IStockRepository stockRepository, 
        IBaseRepository<Entities.Warehouse> warehouseRepository, 
        IBaseRepository<Entities.Product> productRepository
    )
    {
        _stockRepository = stockRepository;
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
    }

    public async Task Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        var stock = await _stockRepository.GetAsync(request.WarehouseUid, request.ProductUid);

        if (stock == null)
        {
            var warehouse = await _warehouseRepository.GetByUidAsync(request.WarehouseUid)
                            ?? throw new NotFoundException("Warehouse", request.WarehouseUid.ToString());

            var product = await _productRepository.GetByUidAsync(request.ProductUid)
                          ?? throw new NotFoundException("Product", request.ProductUid.ToString());

            stock = new Stock
            {
                Warehouse = warehouse, 
                Product = product,      
                Quantity = request.NewQuantity
            };

            await _stockRepository.AddAsync(stock);
        }
        else
        {
            stock.Quantity = request.NewQuantity;
            await _stockRepository.UpdateAsync(stock);
        }
    }
}
