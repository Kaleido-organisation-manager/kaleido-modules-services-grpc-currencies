using Kaleido.Modules.Services.Grpc.Currencies.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Common.Validators;

public class KeyValidatorTests
{
    private readonly KeyValidator _validator;

    public KeyValidatorTests()
    {
        _validator = new KeyValidator();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Validate_WhenKeyIsNullOrWhitespace_ShouldReturnFalse(string key)
    {
        // Act
        var result = _validator.Validate(key);

        // Assert
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("a")]
    public void Validate_WhenKeyIsShorterThanMinLength_ShouldReturnFalse(string key)
    {
        // Act
        var result = _validator.Validate(key);

        // Assert
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz")]  // 52 characters
    public void Validate_WhenKeyIsLongerThanMaxLength_ShouldReturnFalse(string key)
    {
        // Act
        var result = _validator.Validate(key);

        // Assert
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")] // should be a guid
    public void Validate_WhenKeyIsValid_ShouldReturnTrue(string key)
    {
        // Act
        var result = _validator.Validate(key);

        // Assert
        Assert.True(result.IsValid);
    }
}