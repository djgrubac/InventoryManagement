using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Application.Common.Models;
using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;
using System.Linq;


namespace Microsoft.Extensions.DependencyInjection.WarehouseStock;

public class StockService : IStockService
{
    private readonly IStockRepository _stockRepository;
    private readonly IBaseRepository<Entities.Warehouse> _warehouseRepository;
    private readonly IBaseRepository<Entities.Warehouse> _productRepository;

    public StockService
    (
        IStockRepository stockRepository, 
        IBaseRepository<Entities.Warehouse> warehouseRepository, 
        IBaseRepository<Entities.Warehouse> productRepository
    )
    {
        _stockRepository = stockRepository;
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
    }

    public async Task<StockDTO?> GetAsync(Guid warehouseUid, Guid productUid)
    {
        var stock = await _stockRepository.GetAsync(warehouseUid, productUid);
        if (stock == null)
            return null;

        return new StockDTO
        {
            Warehouse = new WarehouseDTO
            {
                Uid = stock.Warehouse.Uid,
                Name = stock.Warehouse.Name,
                Address = stock.Warehouse.Address,
                ContactPerson = stock.Warehouse.ContactPerson
            },
            Product = new ProductDTO
            {
                Uid = stock.Product.Uid,
                Name = stock.Product.Name,
                Price = stock.Product.Price,
                StockQuantity = stock.Product.StockQuantity,
                Description = stock.Product.Description
            },
            Quantity = stock.Quantity
        };
    }

    public async Task<IEnumerable<StockDTO>> GetForWarehouseAsync(Guid warehouseUid)
    {
        var items = await _stockRepository.GetByWarehouseAsync(warehouseUid);

        return items
            .Where(s => s != null)
            .Select(s => new StockDTO
            {
                Warehouse = new WarehouseDTO
                {
                    Uid = s!.Warehouse.Uid,
                    Name = s.Warehouse.Name,
                    Address = s.Warehouse.Address,
                    ContactPerson = s.Warehouse.ContactPerson
                },
                Product = new ProductDTO
                {
                    Uid = s.Product.Uid,
                    Name = s.Product.Name,
                    Price = s.Product.Price,
                    StockQuantity = s.Product.StockQuantity,
                    Description = s.Product.Description
                },
                Quantity = s.Quantity
            });
    }

    public async Task SetQuantityAsync(Guid warehouseUid, Guid productUid, int quantity)
    {
        var existing = await _stockRepository.GetAsync(warehouseUid, productUid);

        if (existing == null)
        {
            var warehouse = await _warehouseRepository.GetByUidAsync(warehouseUid);
            var product = await _productRepository.GetByUidAsync(productUid);
            
            // Null checks
            if (warehouse == null)
                throw new NotFoundException(nameof(Warehouse), warehouseUid.ToString());
        
            if (product == null)
                throw new NotFoundException(nameof(Product), productUid.ToString());
        
            var newStock = new Entities.Stock
            {
                WarehouseId = warehouse.Id,
                ProductId = product.Id,
                Quantity = quantity
            };

            await _stockRepository.AddAsync(newStock);
        }
        else
        {
            existing.Quantity = quantity;
            await _stockRepository.UpdateAsync(existing);
        }
    }
}

