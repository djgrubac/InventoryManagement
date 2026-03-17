using FluentValidation;

namespace Microsoft.Extensions.DependencyInjection.Order.Commands.ChangeStatus;

public class ChangeOrderStatusCommandValidator : AbstractValidator<ChangeOrderStatusCommand>
{
    public ChangeOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderUid)
            .NotEmpty();

        RuleFor(x => x.NewStatus)
            .IsInEnum();
    }
}

