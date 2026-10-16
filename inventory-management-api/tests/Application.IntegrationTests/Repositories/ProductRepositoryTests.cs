using Inventory_Management.Infrastructure.Data;

namespace Inventory_Management.Application.IntegrationTests.Repositories;

public class ProductRepositoryTests : IntegrationTestBase
{
    [Test]
    public async Task AddAsync_PersistsProduct_GetByUidIncludesCategory()
    {
        var category = TestData.Category("Tools");
        await SeedAsync(category);
        var product = new Product { Name = "Saw", Price = 12.5m, CategoryId = category.Id, Description = "Sharp" };

        await new ProductRepository(CreateContext()).AddAsync(product);
        var loaded = await new ProductRepository(CreateContext()).GetByUidAsync(product.Uid);

        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be("Saw");
        loaded.Price.Should().Be(12.5m);
        loaded.Category.Should().NotBeNull();
        loaded.Category.Uid.Should().Be(category.Uid);
    }

    [Test]
    public async Task GetByUidAsync_UnknownUid_ReturnsNull()
    {
        await SeedAsync(TestData.Product(TestData.Category()));

        var loaded = await new ProductRepository(CreateContext()).GetByUidAsync(Guid.NewGuid());

        loaded.Should().BeNull();
    }

    [Test]
    public async Task GetAllAsync_IncludesCategoryForEveryProduct()
    {
        var tools = TestData.Category("Tools");
        var garden = TestData.Category("Garden");
        await SeedAsync(TestData.Product(tools, "Hammer"), TestData.Product(garden, "Rake"));

        var all = (await new ProductRepository(CreateContext()).GetAllAsync()).ToList();

        all.Should().HaveCount(2);
        all.Single(p => p.Name == "Hammer").Category.Caption.Should().Be("Tools");
        all.Single(p => p.Name == "Rake").Category.Caption.Should().Be("Garden");
    }

    [Test]
    public async Task UpdateAsync_PersistsChanges()
    {
        var product = TestData.Product(TestData.Category(), "Hammer", 10m);
        await SeedAsync(product);
        var repository = new ProductRepository(CreateContext());
        var toUpdate = (await repository.GetByUidAsync(product.Uid))!;

        toUpdate.Name = "Sledgehammer";
        toUpdate.Price = 30m;
        await repository.UpdateAsync(toUpdate);

        var loaded = await new ProductRepository(CreateContext()).GetByUidAsync(product.Uid);
        loaded!.Name.Should().Be("Sledgehammer");
        loaded.Price.Should().Be(30m);
    }

    [Test]
    public async Task UpdateAsync_DetachedEntity_PersistsChanges()
    {
        var product = TestData.Product(TestData.Category(), "Hammer", 10m);
        await SeedAsync(product);
        var detached = await CreateContext().Products.AsNoTracking().SingleAsync(p => p.Uid == product.Uid);

        detached.Price = 11m;
        await new ProductRepository(CreateContext()).UpdateAsync(detached);

        var loaded = await new ProductRepository(CreateContext()).GetByUidAsync(product.Uid);
        loaded!.Price.Should().Be(11m);
    }

    [Test]
    public async Task DeleteAsync_RemovesProduct_KeepsCategory()
    {
        var category = TestData.Category();
        var product = TestData.Product(category);
        await SeedAsync(product);

        await new ProductRepository(CreateContext()).DeleteAsync(product.Uid);

        var context = CreateContext();
        (await context.Products.AnyAsync(p => p.Uid == product.Uid)).Should().BeFalse();
        (await context.Categories.AnyAsync(c => c.Uid == category.Uid)).Should().BeTrue();
    }

    [Test]
    public async Task DeleteAsync_UnknownUid_DoesNothing()
    {
        await SeedAsync(TestData.Product(TestData.Category()));

        await new ProductRepository(CreateContext()).DeleteAsync(Guid.NewGuid());

        (await CreateContext().Products.CountAsync()).Should().Be(1);
    }

    [Test]
    [Ignore("Known bug: OrderItem->Product is DeleteBehavior.Cascade, so deleting a product silently deletes historical order lines. Needs a Restrict (or soft delete) mapping plus a migration.")]
    public async Task DeleteAsync_ProductOnExistingOrder_KeepsOrderHistory()
    {
        var product = TestData.Product(TestData.Category(), price: 5m);
        var order = TestData.Order(TestData.Warehouse(), items: (product, 2));
        await SeedAsync(order);

        try
        {
            await new ProductRepository(CreateContext()).DeleteAsync(product.Uid);
        }
        catch (DbUpdateException)
        {
            // Refusing the delete is an acceptable outcome; losing the order line is not.
        }

        (await CreateContext().OrderItems.CountAsync(i => i.OrderId == order.Id)).Should().Be(1);
    }
}
