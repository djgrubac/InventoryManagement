using FluentValidation.TestHelper;
using Inventory_Management.Domain.Enums;
using Microsoft.Extensions.DependencyInjection.Order.Commands.AddItem;
using Microsoft.Extensions.DependencyInjection.Order.Commands.ChangeStatus;
using Microsoft.Extensions.DependencyInjection.Order.Commands.Create;
using Microsoft.Extensions.DependencyInjection.Order.Commands.RemoveItem;

namespace Inventory_Management.Application.UnitTests.Order.Commands;

public class OrderValidatorTests
{
    [Test]
    public void OrderCreate_ValidCommand_HasNoErrors()
    {
        new OrderCreateCommandValidator()
            .TestValidate(new OrderCreateCommand(OrderType.Incoming, Guid.NewGuid()))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void OrderCreate_EmptyWarehouseUid_HasError()
    {
        new OrderCreateCommandValidator()
            .TestValidate(new OrderCreateCommand(OrderType.Incoming, Guid.Empty))
            .ShouldHaveValidationErrorFor(c => c.WarehouseUid);
    }

    [Test]
    public void AddItem_ValidCommand_HasNoErrors()
    {
        new AddOrderItemCommandValidator()
            .TestValidate(new AddOrderItemCommand(Guid.NewGuid(), Guid.NewGuid(), 1))
            .ShouldNotHaveAnyValidationErrors();
    }

    [TestCase(0)]
    [TestCase(-3)]
    public void AddItem_NonPositiveQuantity_HasError(int quantity)
    {
        new AddOrderItemCommandValidator()
            .TestValidate(new AddOrderItemCommand(Guid.NewGuid(), Guid.NewGuid(), quantity))
            .ShouldHaveValidationErrorFor(c => c.Quantity);
    }

    [Test]
    public void AddItem_EmptyUids_HaveErrors()
    {
        var result = new AddOrderItemCommandValidator()
            .TestValidate(new AddOrderItemCommand(Guid.Empty, Guid.Empty, 1));

        result.ShouldHaveValidationErrorFor(c => c.OrderUid);
        result.ShouldHaveValidationErrorFor(c => c.ProductUid);
    }

    [Test]
    public void RemoveItem_EmptyUids_HaveErrors()
    {
        var result = new RemoveOrderItemCommandValidator()
            .TestValidate(new RemoveOrderItemCommand(Guid.Empty, Guid.Empty));

        result.ShouldHaveValidationErrorFor(c => c.OrderUid);
        result.ShouldHaveValidationErrorFor(c => c.ProductUid);
    }

    [Test]
    public void ChangeStatus_ValidCommand_HasNoErrors()
    {
        new ChangeOrderStatusCommandValidator()
            .TestValidate(new ChangeOrderStatusCommand(Guid.NewGuid(), OrderStatus.Completed))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void ChangeStatus_UndefinedStatus_HasError()
    {
        new ChangeOrderStatusCommandValidator()
            .TestValidate(new ChangeOrderStatusCommand(Guid.NewGuid(), (OrderStatus)99))
            .ShouldHaveValidationErrorFor(c => c.NewStatus);
    }

    [Test]
    public void ChangeStatus_EmptyOrderUid_HasError()
    {
        new ChangeOrderStatusCommandValidator()
            .TestValidate(new ChangeOrderStatusCommand(Guid.Empty, OrderStatus.Cancelled))
            .ShouldHaveValidationErrorFor(c => c.OrderUid);
    }
}
