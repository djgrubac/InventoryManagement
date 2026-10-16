using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Domain.Enums;
using InventoryManagement.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection.Order.Commands.Create;
using Entities = Inventory_Management.Domain.Entities;

namespace Inventory_Management.Application.UnitTests.Order.Commands;

public class OrderCreateCommandHandlerTests
{
    private Mock<IOrderRepository> _orderRepository = null!;
    private Mock<IBaseRepository<Entities.Warehouse>> _warehouseRepository = null!;
    private OrderCreateCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _orderRepository = new Mock<IOrderRepository>();
        _warehouseRepository = new Mock<IBaseRepository<Entities.Warehouse>>();
        _handler = new OrderCreateCommandHandler(_orderRepository.Object, _warehouseRepository.Object);
    }

    [Test]
    public async Task Handle_WarehouseExists_AddsPendingOrderAndReturnsUid()
    {
        var warehouse = new Entities.Warehouse { Id = 1, Name = "Main" };
        _warehouseRepository.Setup(r => r.GetByUidAsync(warehouse.Uid)).ReturnsAsync(warehouse);
        Entities.Order? added = null;
        _orderRepository.Setup(r => r.AddAsync(It.IsAny<Entities.Order>()))
            .Callback<Entities.Order>(o => added = o);

        var uid = await _handler.Handle(new OrderCreateCommand(OrderType.Incoming, warehouse.Uid), CancellationToken.None);

        added.Should().NotBeNull();
        added!.Uid.Should().Be(uid);
        added.Status.Should().Be(OrderStatus.Pending);
        added.Type.Should().Be(OrderType.Incoming);
        added.Warehouse.Should().BeSameAs(warehouse);
        added.Items.Should().BeEmpty();
        added.TotalPrice.Should().Be(0m);
    }

    [Test]
    public async Task Handle_WarehouseMissing_ThrowsAndDoesNotAddOrder()
    {
        var act = () => _handler.Handle(new OrderCreateCommand(OrderType.Outgoing, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
        _orderRepository.Verify(r => r.AddAsync(It.IsAny<Entities.Order>()), Times.Never);
    }
}
