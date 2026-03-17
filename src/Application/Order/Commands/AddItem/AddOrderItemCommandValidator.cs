using FluentValidation;

namespace Microsoft.Extensions.DependencyInjection.Order.Commands.AddItem;

public class AddOrderItemCommandValidator : AbstractValidator<AddOrderItemCommand>
{
    public AddOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderUid)
            .NotEmpty();

        RuleFor(x => x.ProductUid)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}

