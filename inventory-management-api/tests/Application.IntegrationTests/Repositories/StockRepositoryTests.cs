using Inventory_Management.Infrastructure.Repositories;

namespace Inventory_Management.Application.IntegrationTests.Repositories;

public class StockRepositoryTests : IntegrationTestBase
{
    [Test]
    public async Task GetAsync_ExistingPair_IncludesProductAndWarehouse()
    {
        var category = TestData.Category();
        var hammer = TestData.Product(category, "Hammer");
        var saw = TestData.Product(category, "Saw");
        var north = TestData.Warehouse("North");
        var south = TestData.Warehouse("South");
        await SeedAsync(
            TestData.Stock(hammer, north, 5),
            TestData.Stock(saw, north, 7),
            TestData.Stock(hammer, south, 9));

        var stock = await new StockRepository(CreateContext()).GetAsync(south.Uid, hammer.Uid);

        stock.Should().NotBeNull();
        stock!.Quantity.Should().Be(9);
        stock.Warehouse.Uid.Should().Be(south.Uid);
        stock.Product.Uid.Should().Be(hammer.Uid);
    }

    [Test]
    public async Task GetAsync_NoStockForPair_ReturnsNull()
    {
        var category = TestData.Category();
        var hammer = TestData.Product(category, "Hammer");
        var saw = TestData.Product(category, "Saw");
        var north = TestData.Warehouse("North");
        await SeedAsync(TestData.Stock(hammer, north, 5), saw);

        var stock = await new StockRepository(CreateContext()).GetAsync(north.Uid, saw.Uid);

        stock.Should().BeNull();
    }

    [Test]
    public async Task GetByWarehouseAsync_ReturnsOnlyThatWarehouse_WithNavigations()
    {
        var category = TestData.Category();
        var hammer = TestData.Product(category, "Hammer");
        var saw = TestData.Product(category, "Saw");
        var north = TestData.Warehouse("North");
        var south = TestData.Warehouse("South");
        await SeedAsync(
            TestData.Stock(hammer, north, 5),
            TestData.Stock(saw, north, 7),
            TestData.Stock(hammer, south, 9));

        var stocks = (await new StockRepository(CreateContext()).GetByWarehouseAsync(north.Uid)).ToList();

        stocks.Should().HaveCount(2);
        stocks.Should().OnlyContain(s => s.Warehouse.Uid == north.Uid);
        stocks.Select(s => s.Product.Name).Should().BeEquivalentTo("Hammer", "Saw");
    }

    [Test]
    public async Task AddAsync_PersistsStock()
    {
        var product = TestData.Product(TestData.Category());
        var warehouse = TestData.Warehouse();
        await SeedAsync(product, warehouse);

        await new StockRepository(CreateContext()).AddAsync(
            new Stock { ProductId = product.Id, WarehouseId = warehouse.Id, Quantity = 3 });

        var loaded = await new StockRepository(CreateContext()).GetAsync(warehouse.Uid, product.Uid);
        loaded!.Quantity.Should().Be(3);
    }

    [Test]
    public async Task UpdateAsync_PersistsQuantity()
    {
        var product = TestData.Product(TestData.Category());
        var warehouse = TestData.Warehouse();
        await SeedAsync(TestData.Stock(product, warehouse, 3));
        var repository = new StockRepository(CreateContext());
        var stock = (await repository.GetAsync(warehouse.Uid, product.Uid))!;

        stock.Quantity = 8;
        await repository.UpdateAsync(stock);

        var loaded = await new StockRepository(CreateContext()).GetAsync(warehouse.Uid, product.Uid);
        loaded!.Quantity.Should().Be(8);
    }

    [Test]
    public async Task Query_IncludesProductAndWarehouse()
    {
        var product = TestData.Product(TestData.Category(), "Hammer");
        var warehouse = TestData.Warehouse("North");
        await SeedAsync(TestData.Stock(product, warehouse, 3));

        var stocks = await new StockRepository(CreateContext()).Query().ToListAsync();

        stocks.Should().ContainSingle();
        stocks[0].Product.Name.Should().Be("Hammer");
        stocks[0].Warehouse.Name.Should().Be("North");
    }

    [Test]
    public async Task AnyOnHandForProductAsync_IgnoresEmptyRowsAndOtherProducts()
    {
        var category = TestData.Category();
        var hammer = TestData.Product(category, "Hammer");
        var saw = TestData.Product(category, "Saw");
        var warehouse = TestData.Warehouse();
        await SeedAsync(TestData.Stock(hammer, warehouse, 0), TestData.Stock(saw, warehouse, 4));
        var repository = new StockRepository(CreateContext());

        (await repository.AnyOnHandForProductAsync(hammer.Id)).Should().BeFalse();
        (await repository.AnyOnHandForProductAsync(saw.Id)).Should().BeTrue();
    }
}
