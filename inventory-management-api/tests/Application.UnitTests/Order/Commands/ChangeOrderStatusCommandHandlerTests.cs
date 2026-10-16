using Ardalis.GuardClauses;
using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Domain.Enums;
using Microsoft.Extensions.DependencyInjection.Order.Commands.ChangeStatus;
using ValidationException = Inventory_Management.Application.Common.Exceptions.ValidationException;
using Entities = Inventory_Management.Domain.Entities;

namespace Inventory_Management.Application.UnitTests.Order.Commands;

public class ChangeOrderStatusCommandHandlerTests
{
    private Mock<IOrderRepository> _orderRepository = null!;
    private Mock<IStockRepository> _stockRepository = null!;
    private ChangeOrderStatusCommandHandler _handler = null!;
    private Entities.Warehouse _warehouse = null!;

    [SetUp]
    public void Setup()
    {
        _orderRepository = new Mock<IOrderRepository>();
        _stockRepository = new Mock<IStockRepository>();
        _handler = new ChangeOrderStatusCommandHandler(_orderRepository.Object, _stockRepository.Object);
        _warehouse = new Entities.Warehouse { Id = 7, Name = "Main" };
    }

    [Test]
    public async Task Handle_PendingToCancelled_UpdatesStatusWithoutTouchingStock()
    {
        var order = SetupOrder(OrderStatus.Pending, OrderType.Outgoing, (CreateProduct(1), 2));

        await _handler.Handle(new ChangeOrderStatusCommand(order.Uid, OrderStatus.Cancelled), CancellationToken.None);

        order.Status.Should().Be(OrderStatus.Cancelled);
        _orderRepository.Verify(r => r.UpdateAsync(order), Times.Once);
        _stockRepository.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Handle_OrderNotFound_ThrowsNotFoundException()
    {
        var act = () => _handler.Handle(new ChangeOrderStatusCommand(Guid.NewGuid(), OrderStatus.Cancelled), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [TestCase(OrderStatus.Completed)]
    [TestCase(OrderStatus.Cancelled)]
    public async Task Handle_OrderAlreadyTerminal_ThrowsValidationException(OrderStatus status)
    {
        var order = SetupOrder(status, OrderType.Incoming, (CreateProduct(1), 2));

        var act = () => _handler.Handle(new ChangeOrderStatusCommand(order.Uid, OrderStatus.Pending), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        order.Status.Should().Be(status);
        _orderRepository.Verify(r => r.UpdateAsync(It.IsAny<Entities.Order>()), Times.Never);
    }

    [Test]
    public async Task Handle_CompleteEmptyOrder_ThrowsValidationException()
    {
        var order = SetupOrder(OrderStatus.Pending, OrderType.Incoming);

        var act = () => _handler.Handle(new ChangeOrderStatusCommand(order.Uid, OrderStatus.Completed), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Test]
    public async Task Handle_CompleteIncomingWithoutStock_AddsNewStock()
    {
        var product = CreateProduct(1);
        var order = SetupOrder(OrderStatus.Pending, OrderType.Incoming, (product, 5));
        Entities.Stock? added = null;
        _stockRepository.Setup(r => r.AddAsync(It.IsAny<Entities.Stock>()))
            .Callback<Entities.Stock>(s => added = s);

        await _handler.Handle(new ChangeOrderStatusCommand(order.Uid, OrderStatus.Completed), CancellationToken.None);

        added.Should().NotBeNull();
        added!.WarehouseId.Should().Be(_warehouse.Id);
        added.ProductId.Should().Be(product.Id);
        added.Quantity.Should().Be(5);
        order.Status.Should().Be(OrderStatus.Completed);
        _orderRepository.Verify(r => r.UpdateAsync(order), Times.Once);
    }

    [Test]
    public async Task Handle_CompleteIncomingWithExistingStock_IncreasesQuantity()
    {
        var product = CreateProduct(1);
        var order = SetupOrder(OrderStatus.Pending, OrderType.Incoming, (product, 5));
        var stock = SetupStock(product, quantity: 10);

        await _handler.Handle(new ChangeOrderStatusCommand(order.Uid, OrderStatus.Completed), CancellationToken.None);

        stock.Quantity.Should().Be(15);
        _stockRepository.Verify(r => r.UpdateAsync(stock), Times.Once);
        _stockRepository.Verify(r => r.AddAsync(It.IsAny<Entities.Stock>()), Times.Never);
    }

    [Test]
    public async Task Handle_CompleteOutgoingWithEnoughStock_DecreasesQuantity()
    {
        var product = CreateProduct(1);
        var order = SetupOrder(OrderStatus.Pending, OrderType.Outgoing, (product, 4));
        var stock = SetupStock(product, quantity: 10);

        await _handler.Handle(new ChangeOrderStatusCommand(order.Uid, OrderStatus.Completed), CancellationToken.None);

        stock.Quantity.Should().Be(6);
        order.Status.Should().Be(OrderStatus.Completed);
        _stockRepository.Verify(r => r.UpdateAsync(stock), Times.Once);
    }

    [Test]
    public async Task Handle_CompleteOutgoingWithInsufficientStockOnLaterLine_ChangesNothing()
    {
        var first = CreateProduct(1);
        var second = CreateProduct(2);
        var order = SetupOrder(OrderStatus.Pending, OrderType.Outgoing, (first, 2), (second, 9));
        var firstStock = SetupStock(first, quantity: 10);
        var secondStock = SetupStock(second, quantity: 3);

        var act = () => _handler.Handle(new ChangeOrderStatusCommand(order.Uid, OrderStatus.Completed), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        firstStock.Quantity.Should().Be(10);
        secondStock.Quantity.Should().Be(3);
        order.Status.Should().Be(OrderStatus.Pending);
        _stockRepository.Verify(r => r.UpdateAsync(It.IsAny<Entities.Stock>()), Times.Never);
        _orderRepository.Verify(r => r.UpdateAsync(It.IsAny<Entities.Order>()), Times.Never);
    }

    [Test]
    public async Task Handle_CompleteOutgoingWithoutStock_ThrowsValidationException()
    {
        var order = SetupOrder(OrderStatus.Pending, OrderType.Outgoing, (CreateProduct(1), 1));

        var act = () => _handler.Handle(new ChangeOrderStatusCommand(order.Uid, OrderStatus.Completed), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    private static Entities.Product CreateProduct(int id) =>
        new() { Id = id, Name = $"Product {id}", Price = 1m };

    private Entities.Order SetupOrder(OrderStatus status, OrderType type, params (Entities.Product Product, int Quantity)[] lines)
    {
        var order = new Entities.Order
        {
            Id = 1,
            Status = status,
            Type = type,
            Warehouse = _warehouse,
            WarehouseId = _warehouse.Id
        };
        foreach (var (product, quantity) in lines)
            order.Items.Add(new Entities.OrderItem { Product = product, ProductId = product.Id, Quantity = quantity, Price = product.Price });

        _orderRepository.Setup(r => r.GetByUidAsync(order.Uid)).ReturnsAsync(order);
        return order;
    }

    private Entities.Stock SetupStock(Entities.Product product, int quantity)
    {
        var stock = new Entities.Stock
        {
            Product = product,
            ProductId = product.Id,
            Warehouse = _warehouse,
            WarehouseId = _warehouse.Id,
            Quantity = quantity
        };
        _stockRepository.Setup(r => r.GetAsync(_warehouse.Uid, product.Uid, It.IsAny<CancellationToken>())).ReturnsAsync(stock);
        return stock;
    }
}
