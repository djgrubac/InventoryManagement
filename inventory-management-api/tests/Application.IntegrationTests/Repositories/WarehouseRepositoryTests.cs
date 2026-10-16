using Inventory_Management.Infrastructure.Data;

namespace Inventory_Management.Application.IntegrationTests.Repositories;

public class WarehouseRepositoryTests : IntegrationTestBase
{
    [Test]
    public async Task AddAsync_PersistsWarehouse_ReadableByUid()
    {
        var warehouse = TestData.Warehouse("North", "Acme");

        await new WarehouseRepository(CreateContext()).AddAsync(warehouse);
        var loaded = await new WarehouseRepository(CreateContext()).GetByUidAsync(warehouse.Uid);

        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be("North");
        loaded.Company.Should().Be("Acme");
        loaded.Address.Should().Be("North street 1");
        loaded.ContactPerson.Should().Be("Jane");
    }

    [Test]
    public async Task GetByUidAsync_UnknownUid_ReturnsNull()
    {
        await SeedAsync(TestData.Warehouse());

        var loaded = await new WarehouseRepository(CreateContext()).GetByUidAsync(Guid.NewGuid());

        loaded.Should().BeNull();
    }

    [Test]
    public async Task GetAllAsync_ReturnsAllWarehouses()
    {
        await SeedAsync(TestData.Warehouse("North"), TestData.Warehouse("South"));

        var all = await new WarehouseRepository(CreateContext()).GetAllAsync();

        all.Select(w => w.Name).Should().BeEquivalentTo("North", "South");
    }

    [Test]
    public async Task UpdateAsync_PersistsChanges()
    {
        var warehouse = TestData.Warehouse("North", "Acme");
        await SeedAsync(warehouse);
        var repository = new WarehouseRepository(CreateContext());
        var toUpdate = (await repository.GetByUidAsync(warehouse.Uid))!;

        toUpdate.Company = "Globex";
        await repository.UpdateAsync(toUpdate);

        var loaded = await new WarehouseRepository(CreateContext()).GetByUidAsync(warehouse.Uid);
        loaded!.Company.Should().Be("Globex");
    }

    [Test]
    public async Task DeleteAsync_IsNotImplemented()
    {
        var warehouse = TestData.Warehouse();
        await SeedAsync(warehouse);

        var act = () => new WarehouseRepository(CreateContext()).DeleteAsync(warehouse.Uid);

        await act.Should().ThrowAsync<NotImplementedException>();
    }
}
