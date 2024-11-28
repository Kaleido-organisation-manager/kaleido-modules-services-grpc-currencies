using Kaleido.Modules.Services.Grpc.Currencies.Common.Validators;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Common.Validators;

public class NameValidatorTests
{
    private readonly NameValidator _validator;

    public NameValidatorTests()
    {
        _validator = new NameValidator();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Validate_WhenNameIsNullOrWhitespace_ShouldReturnFalse(string name)
    {
        // Act
        var result = _validator.Validate(name);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenNameExceedsMaxLength_ShouldReturnFalse()
    {
        // Arrange
        var longName = new string('a', 101); // 101 characters, exceeding the 100 character limit

        // Act
        var result = _validator.Validate(longName);

        // Assert
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Valid Name")]
    [InlineData("Test Currency")]
    [InlineData("A")]  // Single character is valid
    [InlineData("This is a valid name that is exactly 100 characters long 1234567890123456789012345678901234567890123")]
    public void Validate_WhenNameIsValid_ShouldReturnTrue(string name)
    {
        // Act
        var result = _validator.Validate(name);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenNameIsExactlyMaxLength_ShouldReturnTrue()
    {
        // Arrange
        var maxLengthName = new string('a', 100); // Exactly 100 characters

        // Act
        var result = _validator.Validate(maxLengthName);

        // Assert
        Assert.True(result.IsValid);
    }
}