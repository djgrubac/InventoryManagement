using FluentValidation;

namespace Microsoft.Extensions.DependencyInjection.Order.Commands.Create;

public class OrderCreateCommandValidator : AbstractValidator<OrderCreateCommand>
{
    public OrderCreateCommandValidator()
    {
        RuleFor(x => x.WarehouseUid)
            .NotEmpty();
        // OrderType is an enum and we rely on model binding to reject invalid values.
    }
}

