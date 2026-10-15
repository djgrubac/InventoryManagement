using FluentValidation;

namespace Microsoft.Extensions.DependencyInjection.Order.Queries.GetCollection;

public class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
{
    public GetOrdersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Status)
            .IsInEnum();
    }
}
