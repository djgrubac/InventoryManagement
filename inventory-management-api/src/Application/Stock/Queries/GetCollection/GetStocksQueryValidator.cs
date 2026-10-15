using FluentValidation;

namespace Microsoft.Extensions.DependencyInjection.WarehouseStock.Queries.GetCollection;

public class GetStocksQueryValidator : AbstractValidator<GetStocksQuery>
{
    public GetStocksQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
