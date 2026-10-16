using Inventory_Management.Infrastructure.Data;

namespace Inventory_Management.Application.IntegrationTests.Repositories;

public class OrderItemRepositoryTests : IntegrationTestBase
{
    [Test]
    public async Task AnyForProductAsync_TrueOnlyForOrderedProduct()
    {
        var category = TestData.Category();
        var ordered = TestData.Product(category, "Hammer");
        var unordered = TestData.Product(category, "Saw");
        await SeedAsync(TestData.Order(TestData.Warehouse(), items: (ordered, 1)), unordered);
        var repository = new OrderItemRepository(CreateContext());

        (await repository.AnyForProductAsync(ordered.Id)).Should().BeTrue();
        (await repository.AnyForProductAsync(unordered.Id)).Should().BeFalse();
    }
}
