using FluentValidation;
using Kaleido.Grpc.Currencies;

namespace Kaleido.Modules.Services.Grpc.Currencies.Common.Validators;

public class CurrencyValidator : AbstractValidator<Currency>
{
    public CurrencyValidator()
    {
        RuleFor(c => c.Name).SetValidator(new NameValidator());
        RuleFor(c => c.Code).NotNull().NotEmpty().Length(1, 3);
        RuleFor(c => c.Symbol).Length(1, 10);
        RuleFor(c => c.Denominations).Must(denominations => denominations.Select(d => d.Value).Distinct().Count() == denominations.Count)
            .WithMessage("Denominations must have unique values");
        RuleForEach(c => c.Denominations).SetValidator(new DenominationValidator());
    }
}
