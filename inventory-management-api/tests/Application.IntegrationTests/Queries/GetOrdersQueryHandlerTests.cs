using Inventory_Management.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection.Order.Queries.GetCollection;

namespace Inventory_Management.Application.IntegrationTests.Queries;

public class GetOrdersQueryHandlerTests : IntegrationTestBase
{
    private Warehouse _north = null!;
    private Warehouse _south = null!;
    private Product _hammer = null!;
    private Order _o1 = null!, _o2 = null!, _o3 = null!, _o4 = null!, _o5 = null!;

    // Five orders, one hour apart (_o1 oldest, _o5 newest), spread over warehouses, types and statuses.
    [SetUp]
    public async Task SeedOrders()
    {
        _north = TestData.Warehouse("North", "Acme");
        _south = TestData.Warehouse("South", "Globex");
        _hammer = TestData.Product(TestData.Category(), "Hammer", 10m);

        _o1 = TestData.Order(_north, OrderType.Incoming, OrderStatus.Completed, At(1), (_hammer, 1));
        _o2 = TestData.Order(_south, OrderType.Outgoing, OrderStatus.Pending, At(2), (_hammer, 2));
        _o3 = TestData.Order(_north, OrderType.Outgoing, OrderStatus.Pending, At(3), (_hammer, 3));
        _o4 = TestData.Order(_south, OrderType.Incoming, OrderStatus.Cancelled, At(4), (_hammer, 4));
        _o5 = TestData.Order(_north, OrderType.Outgoing, OrderStatus.Completed, At(5), (_hammer, 5));

        await SeedAsync(_o1, _o2, _o3, _o4, _o5);
    }

    [Test]
    public async Task Handle_NoFilters_ReturnsAllNewestFirst()
    {
        var result = await Handle(new GetOrdersQuery());

        Uids(result).Should().Equal(_o5.Uid, _o4.Uid, _o3.Uid, _o2.Uid, _o1.Uid);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(1);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Test]
    public async Task Handle_SameCreated_TieBreaksOnIdDescending()
    {
        var first = TestData.Order(_north, created: At(10));
        var second = TestData.Order(_north, created: At(10));
        await SeedAsync(first);
        await SeedAsync(second);

        var result = await Handle(new GetOrdersQuery { PageSize = 2 });

        Uids(result).Should().Equal(second.Uid, first.Uid);
    }

    [Test]
    public async Task Handle_WarehouseFilter_ReturnsOnlyThatWarehouse()
    {
        var result = await Handle(new GetOrdersQuery { WarehouseUid = _north.Uid });

        Uids(result).Should().Equal(_o5.Uid, _o3.Uid, _o1.Uid);
        result.TotalCount.Should().Be(3);
    }

    [Test]
    public async Task Handle_TypeFilter_ReturnsOnlyThatType()
    {
        var result = await Handle(new GetOrdersQuery { Type = OrderType.Incoming });

        Uids(result).Should().Equal(_o4.Uid, _o1.Uid);
    }

    [Test]
    public async Task Handle_StatusFilter_ReturnsOnlyThatStatus()
    {
        var result = await Handle(new GetOrdersQuery { Status = OrderStatus.Pending });

        Uids(result).Should().Equal(_o3.Uid, _o2.Uid);
    }

    [Test]
    public async Task Handle_CombinedFilters_AreAnded()
    {
        var result = await Handle(new GetOrdersQuery
        {
            WarehouseUid = _north.Uid,
            Type = OrderType.Outgoing,
            Status = OrderStatus.Completed
        });

        Uids(result).Should().Equal(_o5.Uid);
        result.TotalCount.Should().Be(1);
    }

    [Test]
    public async Task Handle_FilterMatchesNothing_ReturnsEmptyPage()
    {
        var result = await Handle(new GetOrdersQuery { WarehouseUid = Guid.NewGuid() });

        result.Orders.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [TestCase(1, new[] { 5, 4 })]
    [TestCase(2, new[] { 3, 2 })]
    [TestCase(3, new[] { 1 })]
    public async Task Handle_Paging_ReturnsRequestedSliceWithTotals(int pageNumber, int[] expectedOrders)
    {
        var byNumber = new Dictionary<int, Order> { [1] = _o1, [2] = _o2, [3] = _o3, [4] = _o4, [5] = _o5 };

        var result = await Handle(new GetOrdersQuery { PageNumber = pageNumber, PageSize = 2 });

        Uids(result).Should().Equal(expectedOrders.Select(n => byNumber[n].Uid));
        result.PageNumber.Should().Be(pageNumber);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
    }

    [Test]
    public async Task Handle_PageBeyondEnd_ReturnsNoItemsButKeepsTotals()
    {
        var result = await Handle(new GetOrdersQuery { PageNumber = 4, PageSize = 2 });

        result.Orders.Should().BeEmpty();
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
    }

    [Test]
    public async Task Handle_PagingWithFilter_CountsOnlyFilteredRows()
    {
        var result = await Handle(new GetOrdersQuery { WarehouseUid = _north.Uid, PageNumber = 2, PageSize = 2 });

        Uids(result).Should().Equal(_o1.Uid);
        result.TotalCount.Should().Be(3);
        result.TotalPages.Should().Be(2);
    }

    [Test]
    public async Task Handle_ProjectsWarehouseItemsAndTotals()
    {
        var saw = TestData.Product(TestData.Category("Saws"), "Saw", 25m);
        var order = TestData.Order(_south, OrderType.Outgoing, OrderStatus.Pending, At(20), (_hammer, 2), (saw, 3));
        await SeedAsync(order);

        var result = await Handle(new GetOrdersQuery { PageSize = 1 });

        var dto = result.Orders.Should().ContainSingle().Subject;
        dto.Uid.Should().Be(order.Uid);
        dto.Type.Should().Be(OrderType.Outgoing);
        dto.Status.Should().Be(OrderStatus.Pending);
        dto.Created.Should().Be(At(20));
        dto.TotalPrice.Should().Be(95m);
        dto.Warehouse.Uid.Should().Be(_south.Uid);
        dto.Warehouse.Name.Should().Be("South");
        dto.Warehouse.Company.Should().Be("Globex");
        dto.Items.Select(i => (i.Product.Name, i.Quantity, i.UnitPrice, i.Total))
            .Should().BeEquivalentTo(new[] { ("Hammer", 2, 10m, 20m), ("Saw", 3, 25m, 75m) });
    }

    private async Task<GetOrdersResult> Handle(GetOrdersQuery query) =>
        await new GetOrdersQueryHandler(new OrderRepository(CreateContext())).Handle(query, CancellationToken.None);

    private static IEnumerable<Guid> Uids(GetOrdersResult result) => result.Orders!.Select(o => o.Uid);

    private static DateTimeOffset At(int hours) => TestData.BaseTime.AddHours(hours);
}
