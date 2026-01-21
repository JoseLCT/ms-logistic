using FluentAssertions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Domain.Shared.ValueObjects;
using Xunit;

namespace MsLogistic.UnitTest.Domain.Shared.ValueObjects;

public class PhoneNumberValueTest
{
    #region Create

    [Theory]
    [InlineData("+59171234567")]
    [InlineData("+1234567890")]
    [InlineData("+5491123456789")]
    [InlineData("+447911123456")]
    [InlineData("+8613800138000")]
    public void Create_WithValidE164Format_ShouldSucceed(string validPhone)
    {
        // Act
        var phoneNumber = PhoneNumberValue.Create(validPhone);

        // Assert
        phoneNumber.Should().NotBeNull();
        phoneNumber.Value.Should().Be(validPhone);
    }

    [Theory]
    [InlineData("+591 7123 4567", "+59171234567")]
    [InlineData("+591-7123-4567", "+59171234567")]
    [InlineData("+591 (712) 34567", "+59171234567")]
    [InlineData("+1 (234) 567-8900", "+12345678900")]
    [InlineData("+44 79 1112 3456", "+447911123456")]
    public void Create_WithFormattedNumber_ShouldNormalizeToE164(string input, string expected)
    {
        // Act
        var phoneNumber = PhoneNumberValue.Create(input);

        // Assert
        phoneNumber.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespace_ShouldThrowDomainException(string invalidPhone)
    {
        // Act
        Action act = () => PhoneNumberValue.Create(invalidPhone);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(PhoneNumberErrors.Empty);
    }

    [Theory]
    [InlineData("71234567")]
    [InlineData("+591")]
    [InlineData("+5911234567890123456")]
    [InlineData("+0591712345678")]
    [InlineData("591712345678")]
    [InlineData("++59171234567")]
    [InlineData("+591abc123456")]
    public void Create_WithInvalidFormat_ShouldThrowDomainException(string invalidPhone)
    {
        // Act
        Action act = () => PhoneNumberValue.Create(invalidPhone);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(PhoneNumberErrors.InvalidFormat);
    }

    #endregion
}