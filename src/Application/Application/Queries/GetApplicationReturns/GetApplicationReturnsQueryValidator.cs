using FluentValidation;
using RMS.Application.Common.Interfaces;

namespace RMS.Application.Application.Queries.GetApplicationReturns;

public class GetApplicationReturnsQueryValidator : AbstractValidator<GetApplicationReturnsQuery>
{
    public GetApplicationReturnsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}
