using Ardalis.GuardClauses;
using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Domain.Enums;
using InventoryManagement.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection.Order.Commands.AddItem;
using ValidationException = Inventory_Management.Application.Common.Exceptions.ValidationException;
using Entities = Inventory_Management.Domain.Entities;

namespace Inventory_Management.Application.UnitTests.Order.Commands;

public class AddOrderItemCommandHandlerTests
{
    private Mock<IOrderRepository> _orderRepository = null!;
    private Mock<IBaseRepository<Entities.Product>> _productRepository = null!;
    private AddOrderItemCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _orderRepository = new Mock<IOrderRepository>();
        _productRepository = new Mock<IBaseRepository<Entities.Product>>();
        _handler = new AddOrderItemCommandHandler(_orderRepository.Object, _productRepository.Object);
    }

    [Test]
    public async Task Handle_NewProduct_AddsItemAtProductPriceAndRecalculatesTotal()
    {
        var order = SetupOrder(OrderStatus.Pending);
        var product = SetupProduct(price: 2.5m);

        await _handler.Handle(new AddOrderItemCommand(order.Uid, product.Uid, 4), CancellationToken.None);

        order.Items.Should().ContainSingle();
        order.Items[0].Product.Should().BeSameAs(product);
        order.Items[0].Quantity.Should().Be(4);
        order.Items[0].Price.Should().Be(2.5m);
        order.TotalPrice.Should().Be(10m);
        _orderRepository.Verify(r => r.UpdateAsync(order), Times.Once);
    }

    [Test]
    public async Task Handle_ProductAlreadyInOrder_IncrementsQuantityWithoutDuplicatingLine()
    {
        var order = SetupOrder(OrderStatus.Pending);
        var product = SetupProduct(price: 3m);
        order.Items.Add(new Entities.OrderItem { Product = product, ProductId = product.Id, Quantity = 2, Price = 3m });

        await _handler.Handle(new AddOrderItemCommand(order.Uid, product.Uid, 5), CancellationToken.None);

        order.Items.Should().ContainSingle().Which.Quantity.Should().Be(7);
        order.TotalPrice.Should().Be(21m);
    }

    [Test]
    public async Task Handle_OrderNotFound_ThrowsNotFoundException()
    {
        var act = () => _handler.Handle(new AddOrderItemCommand(Guid.NewGuid(), Guid.NewGuid(), 1), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task Handle_ProductNotFound_ThrowsNotFoundException()
    {
        var order = SetupOrder(OrderStatus.Pending);

        var act = () => _handler.Handle(new AddOrderItemCommand(order.Uid, Guid.NewGuid(), 1), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _orderRepository.Verify(r => r.UpdateAsync(It.IsAny<Entities.Order>()), Times.Never);
    }

    [TestCase(OrderStatus.Completed)]
    [TestCase(OrderStatus.Cancelled)]
    public async Task Handle_OrderNotPending_ThrowsValidationException(OrderStatus status)
    {
        var order = SetupOrder(status);
        var product = SetupProduct(price: 1m);

        var act = () => _handler.Handle(new AddOrderItemCommand(order.Uid, product.Uid, 1), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        order.Items.Should().BeEmpty();
        _orderRepository.Verify(r => r.UpdateAsync(It.IsAny<Entities.Order>()), Times.Never);
    }

    private Entities.Order SetupOrder(OrderStatus status)
    {
        var order = new Entities.Order { Id = 1, Status = status };
        _orderRepository.Setup(r => r.GetByUidAsync(order.Uid)).ReturnsAsync(order);
        return order;
    }

    private Entities.Product SetupProduct(decimal price)
    {
        var product = new Entities.Product { Id = 10, Name = "Widget", Price = price };
        _productRepository.Setup(r => r.GetByUidAsync(product.Uid)).ReturnsAsync(product);
        return product;
    }
}
