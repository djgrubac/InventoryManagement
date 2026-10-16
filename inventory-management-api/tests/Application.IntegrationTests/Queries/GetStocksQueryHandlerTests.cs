using Inventory_Management.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection.WarehouseStock.Queries.GetCollection;

namespace Inventory_Management.Application.IntegrationTests.Queries;

public class GetStocksQueryHandlerTests : IntegrationTestBase
{
    private Category _tools = null!;
    private Category _garden = null!;
    private Product _hammer = null!, _saw = null!, _rake = null!;
    private Warehouse _alpha = null!, _beta = null!;
    private Stock _alphaHammer = null!, _alphaRake = null!, _alphaSaw = null!, _betaHammer = null!, _betaRake = null!;

    // Inserted deliberately out of display order so the test proves the handler sorts
    // (Warehouse.Name, then Product.Name) rather than relying on insertion order.
    [SetUp]
    public async Task SeedStocks()
    {
        _tools = TestData.Category("Tools");
        _garden = TestData.Category("Garden");
        _hammer = TestData.Product(_tools, "Hammer", 10m);
        _saw = TestData.Product(_tools, "Saw", 25m);
        _rake = TestData.Product(_garden, "Rake", 15m);
        _alpha = TestData.Warehouse("Alpha", "Acme");
        _beta = TestData.Warehouse("Beta", "Globex");

        _betaRake = TestData.Stock(_rake, _beta, 5);
        _alphaSaw = TestData.Stock(_saw, _alpha, 3);
        _betaHammer = TestData.Stock(_hammer, _beta, 4);
        _alphaHammer = TestData.Stock(_hammer, _alpha, 1);
        _alphaRake = TestData.Stock(_rake, _alpha, 2);

        await SeedAsync(_betaRake, _alphaSaw, _betaHammer, _alphaHammer, _alphaRake);
    }

    [Test]
    public async Task Handle_NoFilters_OrdersByWarehouseThenProductName()
    {
        var result = await Handle(new GetStocksQuery());

        Keys(result).Should().Equal(
            ("Alpha", "Hammer"), ("Alpha", "Rake"), ("Alpha", "Saw"),
            ("Beta", "Hammer"), ("Beta", "Rake"));
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(1);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Test]
    public async Task Handle_WarehouseFilter_ReturnsOnlyThatWarehouse()
    {
        var result = await Handle(new GetStocksQuery { WarehouseUid = _beta.Uid });

        Keys(result).Should().Equal(("Beta", "Hammer"), ("Beta", "Rake"));
        result.TotalCount.Should().Be(2);
    }

    [Test]
    public async Task Handle_ProductFilter_ReturnsThatProductInEveryWarehouse()
    {
        var result = await Handle(new GetStocksQuery { ProductUid = _rake.Uid });

        Keys(result).Should().Equal(("Alpha", "Rake"), ("Beta", "Rake"));
    }

    [Test]
    public async Task Handle_CategoryFilter_ReturnsProductsInThatCategory()
    {
        var result = await Handle(new GetStocksQuery { CategoryUid = _tools.Uid });

        Keys(result).Should().Equal(("Alpha", "Hammer"), ("Alpha", "Saw"), ("Beta", "Hammer"));
    }

    [Test]
    public async Task Handle_CombinedFilters_AreAnded()
    {
        var result = await Handle(new GetStocksQuery { WarehouseUid = _alpha.Uid, CategoryUid = _garden.Uid });

        Keys(result).Should().Equal(("Alpha", "Rake"));
        result.TotalCount.Should().Be(1);
    }

    [Test]
    public async Task Handle_FilterMatchesNothing_ReturnsEmptyPage()
    {
        var result = await Handle(new GetStocksQuery { ProductUid = Guid.NewGuid() });

        result.Stocks.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Test]
    public async Task Handle_Paging_PagesDoNotOverlapAndCoverEverything()
    {
        var page1 = await Handle(new GetStocksQuery { PageNumber = 1, PageSize = 2 });
        var page2 = await Handle(new GetStocksQuery { PageNumber = 2, PageSize = 2 });
        var page3 = await Handle(new GetStocksQuery { PageNumber = 3, PageSize = 2 });

        Keys(page1).Should().Equal(("Alpha", "Hammer"), ("Alpha", "Rake"));
        Keys(page2).Should().Equal(("Alpha", "Saw"), ("Beta", "Hammer"));
        Keys(page3).Should().Equal(("Beta", "Rake"));
        foreach (var page in new[] { page1, page2, page3 })
        {
            page.TotalCount.Should().Be(5);
            page.TotalPages.Should().Be(3);
            page.PageSize.Should().Be(2);
        }
        page2.PageNumber.Should().Be(2);
    }

    [Test]
    public async Task Handle_PageBeyondEnd_ReturnsNoItemsButKeepsTotals()
    {
        var result = await Handle(new GetStocksQuery { PageNumber = 6, PageSize = 1 });

        result.Stocks.Should().BeEmpty();
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(5);
    }

    [Test]
    public async Task Handle_ProjectsWarehouseProductAndQuantity()
    {
        var result = await Handle(new GetStocksQuery { WarehouseUid = _beta.Uid, ProductUid = _hammer.Uid });

        var dto = result.Stocks.Should().ContainSingle().Subject;
        dto.Quantity.Should().Be(4);
        dto.Warehouse.Uid.Should().Be(_beta.Uid);
        dto.Warehouse.Name.Should().Be("Beta");
        dto.Warehouse.Company.Should().Be("Globex");
        dto.Warehouse.Address.Should().Be("Beta street 1");
        dto.Warehouse.ContactPerson.Should().Be("Jane");
        dto.Product.Uid.Should().Be(_hammer.Uid);
        dto.Product.Name.Should().Be("Hammer");
        dto.Product.Price.Should().Be(10m);
        dto.Product.Description.Should().Be("Hammer description");
    }

    private async Task<GetsStocksResult> Handle(GetStocksQuery query) =>
        await new GetStocksQueryHandler(new StockRepository(CreateContext())).Handle(query, CancellationToken.None);

    private static IEnumerable<(string?, string?)> Keys(GetsStocksResult result) =>
        result.Stocks!.Select(s => (s.Warehouse.Name, s.Product.Name));
}
