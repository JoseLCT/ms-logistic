namespace MsLogistic.Domain.Shared.ValueObjects;

public partial record PhoneNumberValue
{
    public string Number { get; init; }

    public PhoneNumberValue(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            throw new ArgumentException("Phone number cannot be empty.");
        }

        if (number.Length is < 7 or > 15)
        {
            throw new ArgumentException("Phone number must be between 7 and 15 digits.");
        }

        if (!MyRegex().IsMatch(number))
        {
            throw new ArgumentException("Phone number can only contain digits and an optional leading +.");
        }

        Number = number;
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"^\+?[0-9]+$")]
    private static partial System.Text.RegularExpressions.Regex MyRegex();
}