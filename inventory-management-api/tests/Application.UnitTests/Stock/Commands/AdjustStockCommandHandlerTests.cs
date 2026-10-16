using Ardalis.GuardClauses;
using Inventory_Management.Application.Common.Interfaces;
using InventoryManagement.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection.WarehouseStock.Commands;
using Entities = Inventory_Management.Domain.Entities;

namespace Inventory_Management.Application.UnitTests.Stock.Commands;

public class AdjustStockCommandHandlerTests
{
    private Mock<IStockRepository> _stockRepository = null!;
    private Mock<IBaseRepository<Entities.Warehouse>> _warehouseRepository = null!;
    private Mock<IBaseRepository<Entities.Product>> _productRepository = null!;
    private AdjustStockCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _stockRepository = new Mock<IStockRepository>();
        _warehouseRepository = new Mock<IBaseRepository<Entities.Warehouse>>();
        _productRepository = new Mock<IBaseRepository<Entities.Product>>();
        _handler = new AdjustStockCommandHandler(_stockRepository.Object, _warehouseRepository.Object, _productRepository.Object);
    }

    [Test]
    public async Task Handle_StockExists_SetsQuantityAndUpdates()
    {
        var warehouseUid = Guid.NewGuid();
        var productUid = Guid.NewGuid();
        var stock = new Entities.Stock { Quantity = 5 };
        _stockRepository.Setup(r => r.GetAsync(warehouseUid, productUid, It.IsAny<CancellationToken>())).ReturnsAsync(stock);

        await _handler.Handle(new AdjustStockCommand(warehouseUid, productUid, 42), CancellationToken.None);

        stock.Quantity.Should().Be(42);
        _stockRepository.Verify(r => r.UpdateAsync(stock), Times.Once);
        _stockRepository.Verify(r => r.AddAsync(It.IsAny<Entities.Stock>()), Times.Never);
    }

    [Test]
    public async Task Handle_NoStock_AddsNewStockForWarehouseAndProduct()
    {
        var warehouse = new Entities.Warehouse { Id = 1 };
        var product = new Entities.Product { Id = 2 };
        _warehouseRepository.Setup(r => r.GetByUidAsync(warehouse.Uid)).ReturnsAsync(warehouse);
        _productRepository.Setup(r => r.GetByUidAsync(product.Uid)).ReturnsAsync(product);
        Entities.Stock? added = null;
        _stockRepository.Setup(r => r.AddAsync(It.IsAny<Entities.Stock>()))
            .Callback<Entities.Stock>(s => added = s);

        await _handler.Handle(new AdjustStockCommand(warehouse.Uid, product.Uid, 8), CancellationToken.None);

        added.Should().NotBeNull();
        added!.Warehouse.Should().BeSameAs(warehouse);
        added.Product.Should().BeSameAs(product);
        added.Quantity.Should().Be(8);
    }

    [Test]
    public async Task Handle_NoStockAndWarehouseMissing_ThrowsNotFoundException()
    {
        var product = new Entities.Product { Id = 2 };
        _productRepository.Setup(r => r.GetByUidAsync(product.Uid)).ReturnsAsync(product);

        var act = () => _handler.Handle(new AdjustStockCommand(Guid.NewGuid(), product.Uid, 1), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _stockRepository.Verify(r => r.AddAsync(It.IsAny<Entities.Stock>()), Times.Never);
    }

    [Test]
    public async Task Handle_NoStockAndProductMissing_ThrowsNotFoundException()
    {
        var warehouse = new Entities.Warehouse { Id = 1 };
        _warehouseRepository.Setup(r => r.GetByUidAsync(warehouse.Uid)).ReturnsAsync(warehouse);

        var act = () => _handler.Handle(new AdjustStockCommand(warehouse.Uid, Guid.NewGuid(), 1), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _stockRepository.Verify(r => r.AddAsync(It.IsAny<Entities.Stock>()), Times.Never);
    }
}
