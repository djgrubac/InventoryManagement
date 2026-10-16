using Ardalis.GuardClauses;
using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Domain.Enums;
using Microsoft.Extensions.DependencyInjection.Order.Commands.RemoveItem;
using ValidationException = Inventory_Management.Application.Common.Exceptions.ValidationException;
using Entities = Inventory_Management.Domain.Entities;

namespace Inventory_Management.Application.UnitTests.Order.Commands;

public class RemoveOrderItemCommandHandlerTests
{
    private Mock<IOrderRepository> _orderRepository = null!;
    private RemoveOrderItemCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _orderRepository = new Mock<IOrderRepository>();
        _handler = new RemoveOrderItemCommandHandler(_orderRepository.Object);
    }

    [Test]
    public async Task Handle_ItemInOrder_RemovesItemAndRecalculatesTotal()
    {
        var keep = new Entities.Product { Id = 1, Price = 2m };
        var remove = new Entities.Product { Id = 2, Price = 5m };
        var order = SetupOrder(OrderStatus.Pending, (keep, 3), (remove, 1));

        await _handler.Handle(new RemoveOrderItemCommand(order.Uid, remove.Uid), CancellationToken.None);

        order.Items.Should().ContainSingle().Which.Product.Should().BeSameAs(keep);
        order.TotalPrice.Should().Be(6m);
        _orderRepository.Verify(r => r.UpdateAsync(order), Times.Once);
    }

    [Test]
    public async Task Handle_ItemNotInOrder_ThrowsNotFoundException()
    {
        var order = SetupOrder(OrderStatus.Pending, (new Entities.Product { Id = 1, Price = 2m }, 1));

        var act = () => _handler.Handle(new RemoveOrderItemCommand(order.Uid, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        order.Items.Should().HaveCount(1);
    }

    [Test]
    public async Task Handle_OrderNotFound_ThrowsNotFoundException()
    {
        var act = () => _handler.Handle(new RemoveOrderItemCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [TestCase(OrderStatus.Completed)]
    [TestCase(OrderStatus.Cancelled)]
    public async Task Handle_OrderCompletedOrCancelled_ThrowsValidationException(OrderStatus status)
    {
        var product = new Entities.Product { Id = 1, Price = 2m };
        var order = SetupOrder(status, (product, 1));

        var act = () => _handler.Handle(new RemoveOrderItemCommand(order.Uid, product.Uid), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        order.Items.Should().HaveCount(1);
        _orderRepository.Verify(r => r.UpdateAsync(It.IsAny<Entities.Order>()), Times.Never);
    }

    private Entities.Order SetupOrder(OrderStatus status, params (Entities.Product Product, int Quantity)[] lines)
    {
        var order = new Entities.Order { Id = 1, Status = status };
        foreach (var (product, quantity) in lines)
            order.Items.Add(new Entities.OrderItem { Product = product, ProductId = product.Id, Quantity = quantity, Price = product.Price });

        _orderRepository.Setup(r => r.GetByUidAsync(order.Uid)).ReturnsAsync(order);
        return order;
    }
}
