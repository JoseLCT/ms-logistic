using System.Text.RegularExpressions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Shared.Errors;

namespace MsLogistic.Domain.Shared.ValueObjects;

public partial record PhoneNumberValue
{
    public string Value { get; }

    private PhoneNumberValue(string value)
    {
        Value = value;
    }

    public static PhoneNumberValue Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(PhoneNumberErrors.Empty);
        }

        value = NormalizeToE164(value);

        if (!E164Regex().IsMatch(value))
        {
            throw new DomainException(PhoneNumberErrors.InvalidFormat);
        }

        return new PhoneNumberValue(value);
    }

    private static string NormalizeToE164(string value)
    {
        return NormalizeRegex().Replace(value, "");
    }

    [GeneratedRegex(@"^\+[1-9]\d{6,14}$")]
    private static partial Regex E164Regex();

    [GeneratedRegex(@"[\s\-()]")]
    private static partial Regex NormalizeRegex();
}