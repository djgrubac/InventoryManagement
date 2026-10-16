using InventoryManagement.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection.Warehouse.Commands;
using Microsoft.Extensions.DependencyInjection.Warehouse.Commands.Update;
using Microsoft.Extensions.DependencyInjection.Warehouse.Queries;
using Microsoft.Extensions.DependencyInjection.Warehouse.Queries.GetSingle;
using Entities = Inventory_Management.Domain.Entities;

namespace Inventory_Management.Application.UnitTests.Warehouse;

public class WarehouseHandlerTests
{
    private Mock<IBaseRepository<Entities.Warehouse>> _warehouseRepository = null!;

    [SetUp]
    public void Setup()
    {
        _warehouseRepository = new Mock<IBaseRepository<Entities.Warehouse>>();
    }

    [Test]
    public async Task Create_AddsWarehouseAndReturnsResponse()
    {
        Entities.Warehouse? added = null;
        _warehouseRepository.Setup(r => r.AddAsync(It.IsAny<Entities.Warehouse>()))
            .Callback<Entities.Warehouse>(w => added = w);

        var response = await new WarehouseCreateCommandHandler(_warehouseRepository.Object)
            .Handle(new WarehouseCreateCommand("Main", "Street 1", "Ana", "Acme")
            {
                Name = "Main", Address = "Street 1", ContactPerson = "Ana", Company = "Acme"
            }, CancellationToken.None);

        added.Should().NotBeNull();
        added!.Name.Should().Be("Main");
        response.Uid.Should().Be(added.Uid);
        response.Address.Should().Be("Street 1");
        response.ContactPerson.Should().Be("Ana");
        response.Company.Should().Be("Acme");
    }

    [Test]
    public async Task Update_WarehouseExists_UpdatesFields()
    {
        var warehouse = new Entities.Warehouse { Id = 1, Name = "Old", Address = "Old", ContactPerson = "Old", Company = "Old" };
        _warehouseRepository.Setup(r => r.GetByUidAsync(warehouse.Uid)).ReturnsAsync(warehouse);

        await new WarehouseUpdateCommandHandler(_warehouseRepository.Object)
            .Handle(new WarehouseUpdateCommand(warehouse.Uid, "New", "Addr", "Bob", "Corp"), CancellationToken.None);

        warehouse.Name.Should().Be("New");
        warehouse.Address.Should().Be("Addr");
        warehouse.ContactPerson.Should().Be("Bob");
        warehouse.Company.Should().Be("Corp");
        _warehouseRepository.Verify(r => r.UpdateAsync(warehouse), Times.Once);
    }

    [Test]
    public async Task Update_WarehouseMissing_ThrowsKeyNotFoundException()
    {
        var act = () => new WarehouseUpdateCommandHandler(_warehouseRepository.Object)
            .Handle(new WarehouseUpdateCommand(Guid.NewGuid(), "New", null, null, null), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Test]
    public async Task GetAll_MapsEntitiesToDtos()
    {
        var warehouse = new Entities.Warehouse { Id = 1, Name = "Main", Address = "Street 1", ContactPerson = "Ana", Company = "Acme" };
        _warehouseRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { warehouse });

        var result = await new GetWarehousesQueryHandler(_warehouseRepository.Object)
            .Handle(new GetWarehousesQuery(), CancellationToken.None);

        var dto = result.Should().ContainSingle().Subject;
        dto.Uid.Should().Be(warehouse.Uid);
        dto.Name.Should().Be("Main");
        dto.Company.Should().Be("Acme");
    }

    [Test]
    public async Task GetById_WarehouseMissing_ReturnsNull()
    {
        var dto = await new GetWarehouseByIdQueryHandler(_warehouseRepository.Object)
            .Handle(new GetWarehouseByIdQuery(Guid.NewGuid()), CancellationToken.None);

        dto.Should().BeNull();
    }
}
