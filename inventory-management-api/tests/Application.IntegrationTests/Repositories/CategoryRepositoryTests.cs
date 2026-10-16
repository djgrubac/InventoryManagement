using Inventory_Management.Infrastructure.Data;

namespace Inventory_Management.Application.IntegrationTests.Repositories;

public class CategoryRepositoryTests : IntegrationTestBase
{
    [Test]
    public async Task AddAsync_PersistsCategory_ReadableByUid()
    {
        var category = TestData.Category("Garden");

        await new CategoryRepository(CreateContext()).AddAsync(category);
        var loaded = await new CategoryRepository(CreateContext()).GetByUidAsync(category.Uid);

        loaded.Should().NotBeNull();
        loaded!.Id.Should().BePositive();
        loaded.Caption.Should().Be("Garden");
    }

    [Test]
    public async Task GetByUidAsync_UnknownUid_ReturnsNull()
    {
        await SeedAsync(TestData.Category());

        var loaded = await new CategoryRepository(CreateContext()).GetByUidAsync(Guid.NewGuid());

        loaded.Should().BeNull();
    }

    [Test]
    public async Task GetAllAsync_ReturnsAllCategories()
    {
        await SeedAsync(TestData.Category("Tools"), TestData.Category("Garden"));

        var all = await new CategoryRepository(CreateContext()).GetAllAsync();

        all.Select(c => c.Caption).Should().BeEquivalentTo("Tools", "Garden");
    }
}
