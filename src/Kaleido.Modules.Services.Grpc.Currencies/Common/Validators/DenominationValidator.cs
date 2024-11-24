using FluentValidation;
using Kaleido.Grpc.Currencies;

namespace Kaleido.Modules.Services.Grpc.Currencies.Common.Validators;

public class DenominationValidator : AbstractValidator<Denomination>
{
    public DenominationValidator()
    {
        RuleFor(d => d.Value).NotNull().GreaterThan(0);
        RuleFor(d => d.Description).MaximumLength(255);
    }
}
