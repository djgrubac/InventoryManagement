using Inventory_Management.Infrastructure.Data;

namespace Inventory_Management.Application.IntegrationTests.Repositories;

public class OrderRepositoryTests : IntegrationTestBase
{
    private Product _hammer = null!;
    private Product _saw = null!;
    private Warehouse _north = null!;
    private Warehouse _south = null!;

    [SetUp]
    public async Task SeedCatalog()
    {
        var category = TestData.Category();
        _hammer = TestData.Product(category, "Hammer", 10m);
        _saw = TestData.Product(category, "Saw", 25m);
        _north = TestData.Warehouse("North");
        _south = TestData.Warehouse("South");
        await SeedAsync(_hammer, _saw, _north, _south);
    }

    [Test]
    public async Task GetByUidAsync_IncludesWarehouseItemsAndItemProducts()
    {
        var order = TestData.Order(_north, items: new[] { (_hammer, 2), (_saw, 1) });
        await SeedAsync(order);

        var loaded = await new OrderRepository(CreateContext()).GetByUidAsync(order.Uid);

        AssertFullyLoaded(loaded, order, _north);
    }

    [Test]
    public async Task GetByIdAsync_IncludesWarehouseItemsAndItemProducts()
    {
        var order = TestData.Order(_north, items: new[] { (_hammer, 2), (_saw, 1) });
        await SeedAsync(order);

        var loaded = await new OrderRepository(CreateContext()).GetByIdAsync(order.Id);

        AssertFullyLoaded(loaded, order, _north);
    }

    [Test]
    public async Task GetByUidAsync_UnknownUid_ReturnsNull()
    {
        await SeedAsync(TestData.Order(_north));

        var loaded = await new OrderRepository(CreateContext()).GetByUidAsync(Guid.NewGuid());

        loaded.Should().BeNull();
    }

    [Test]
    public async Task GetAllAsync_IncludesNavigationsForEveryOrder()
    {
        await SeedAsync(TestData.Order(_north, items: new[] { (_hammer, 1) }));
        await SeedAsync(TestData.Order(_south, items: new[] { (_saw, 3) }));

        var all = (await new OrderRepository(CreateContext()).GetAllAsync()).ToList();

        all.Should().HaveCount(2);
        all.Should().OnlyContain(o => o.Warehouse != null && o.Items.Count == 1 && o.Items[0].Product != null);
        all.Select(o => o.Warehouse.Name).Should().BeEquivalentTo("North", "South");
    }

    [Test]
    public async Task GetByWarehouseUidAsync_ReturnsOnlyThatWarehouse()
    {
        await SeedAsync(TestData.Order(_north, items: new[] { (_hammer, 1) }));
        await SeedAsync(TestData.Order(_north, items: new[] { (_saw, 1) }));
        await SeedAsync(TestData.Order(_south, items: new[] { (_saw, 3) }));

        var orders = (await new OrderRepository(CreateContext()).GetByWarehouseUidAsync(_north.Uid)).ToList();

        orders.Should().HaveCount(2);
        orders.Should().OnlyContain(o => o.Warehouse.Uid == _north.Uid);
        orders.SelectMany(o => o.Items).Select(i => i.Product.Name).Should().BeEquivalentTo("Hammer", "Saw");
    }

    [Test]
    public async Task AddAsync_PersistsOrderWithItems()
    {
        var order = new Order
        {
            WarehouseId = _north.Id,
            Type = OrderType.Outgoing,
            Status = OrderStatus.Pending,
            TotalPrice = 20m,
            Items = { new OrderItem { ProductId = _hammer.Id, Quantity = 2, Price = 10m } }
        };

        await new OrderRepository(CreateContext()).AddAsync(order);

        var loaded = await new OrderRepository(CreateContext()).GetByUidAsync(order.Uid);
        loaded.Should().NotBeNull();
        loaded!.Type.Should().Be(OrderType.Outgoing);
        loaded.TotalPrice.Should().Be(20m);
        loaded.Items.Should().ContainSingle().Which.Product.Name.Should().Be("Hammer");
    }

    [Test]
    public async Task UpdateAsync_ItemAdded_PersistsNewItem()
    {
        var order = TestData.Order(_north, items: new[] { (_hammer, 1) });
        await SeedAsync(order);
        var context = CreateContext();
        var repository = new OrderRepository(context);
        var tracked = (await repository.GetByUidAsync(order.Uid))!;
        var saw = await context.Products.SingleAsync(p => p.Uid == _saw.Uid);

        tracked.Items.Add(new OrderItem { Product = saw, Quantity = 2, Price = saw.Price });
        tracked.TotalPrice = 60m;
        await repository.UpdateAsync(tracked);

        var loaded = await new OrderRepository(CreateContext()).GetByUidAsync(order.Uid);
        loaded!.TotalPrice.Should().Be(60m);
        loaded.Items.Select(i => (i.Product.Name, i.Quantity)).Should().BeEquivalentTo(new[] { ("Hammer", 1), ("Saw", 2) });
    }

    [Test]
    public async Task UpdateAsync_ItemRemoved_DeletesOrderItemRow()
    {
        // Mirrors OrderService.RemoveItemAsync: load, remove from Items, Update.
        var order = TestData.Order(_north, items: new[] { (_hammer, 1), (_saw, 1) });
        await SeedAsync(order);
        var repository = new OrderRepository(CreateContext());
        var tracked = (await repository.GetByUidAsync(order.Uid))!;

        tracked.Items.Remove(tracked.Items.Single(i => i.Product.Uid == _saw.Uid));
        await repository.UpdateAsync(tracked);

        var context = CreateContext();
        (await context.OrderItems.CountAsync()).Should().Be(1);
        var loaded = await new OrderRepository(context).GetByUidAsync(order.Uid);
        loaded!.Items.Should().ContainSingle().Which.Product.Uid.Should().Be(_hammer.Uid);
    }

    [Test]
    public async Task UpdateAsync_StatusChange_Persists()
    {
        var order = TestData.Order(_north, status: OrderStatus.Pending);
        await SeedAsync(order);
        var repository = new OrderRepository(CreateContext());
        var tracked = (await repository.GetByUidAsync(order.Uid))!;

        tracked.Status = OrderStatus.Completed;
        await repository.UpdateAsync(tracked);

        var loaded = await new OrderRepository(CreateContext()).GetByUidAsync(order.Uid);
        loaded!.Status.Should().Be(OrderStatus.Completed);
    }

    [Test]
    public async Task Query_IsNoTracking()
    {
        await SeedAsync(TestData.Order(_north));
        var context = CreateContext();

        var orders = await new OrderRepository(context).Query().ToListAsync();

        orders.Should().ContainSingle();
        context.ChangeTracker.Entries().Should().BeEmpty();
    }

    private static void AssertFullyLoaded(Order? loaded, Order expected, Warehouse warehouse)
    {
        loaded.Should().NotBeNull();
        loaded!.Uid.Should().Be(expected.Uid);
        loaded.TotalPrice.Should().Be(expected.TotalPrice);
        loaded.Warehouse.Should().NotBeNull();
        loaded.Warehouse.Uid.Should().Be(warehouse.Uid);
        loaded.Items.Should().HaveCount(expected.Items.Count);
        loaded.Items.Should().OnlyContain(i => i.Product != null);
        loaded.Items.Select(i => (i.Product.Uid, i.Quantity, i.Price))
            .Should().BeEquivalentTo(expected.Items.Select(i => (i.Product.Uid, i.Quantity, i.Price)));
    }
}
