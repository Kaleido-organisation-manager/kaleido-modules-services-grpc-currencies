using Kaleido.Modules.Services.Grpc.Currencies.Common.Validators;
using FluentValidation.TestHelper;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Common.Validators
{
    public class CurrencyValidatorTests
    {
        private readonly CurrencyValidator _sut;

        public CurrencyValidatorTests()
        {
            _sut = new CurrencyValidator();
        }

        [Fact]
        public void Validate_ValidCurrency_ShouldNotHaveValidationError()
        {
            // Arrange
            var currency = new CurrencyBuilder().Build();

            // Act
            var result = _sut.TestValidate(currency);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_InvalidName_ShouldHaveValidationError(string name)
        {
            // Arrange
            var currency = new CurrencyBuilder().WithName(name).Build();

            // Act
            var result = _sut.TestValidate(currency);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Validate_NameTooLong_ShouldHaveValidationError()
        {
            // Arrange
            var currency = new CurrencyBuilder().WithName(new string('a', 256)).Build();

            // Act
            var result = _sut.TestValidate(currency);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }
    }
}