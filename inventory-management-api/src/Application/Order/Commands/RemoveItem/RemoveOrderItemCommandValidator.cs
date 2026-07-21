using FluentValidation;

namespace Microsoft.Extensions.DependencyInjection.Order.Commands.RemoveItem;

public class RemoveOrderItemCommandValidator : AbstractValidator<RemoveOrderItemCommand>
{
    public RemoveOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderUid)
            .NotEmpty();

        RuleFor(x => x.ProductUid)
            .NotEmpty();
    }
}

