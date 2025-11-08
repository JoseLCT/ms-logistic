using System.Text;
using System.Text.RegularExpressions;

namespace MsLogistic.Core.Results;

public record Error
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static readonly Error NullValue = new(
        "General.Null",
        "Null value was provided",
        ErrorType.Failure);

    public Error(string code, string? structuredMessage, ErrorType type, params object[]? args)
    {
        if (structuredMessage == null)
        {
            structuredMessage = string.Empty;
        }

        StructuredMessage = structuredMessage;
        Description = BuildMessage(structuredMessage, args);
        Code = code;
        Type = type;
    }

    private static string BuildMessage(string structuredMessage, params object[]? args)
    {
        if (args == null || args.Length == 0)
        {
            return structuredMessage;
        }

        var placeholders = Regex.Matches(structuredMessage, @"\{(\w+)\}");
        StringBuilder result = new(structuredMessage);
        var index = 0;
        foreach (Match match in placeholders)
        {
            if (index >= args.Length)
                break;

            var placeholder = match.Value;
            var valor = args[index]?.ToString() ?? string.Empty;

            result = result.Replace(placeholder, valor);
            index++;
        }

        return result.ToString();
    }

    public string Code { get; }

    public string Description { get; }
    public string StructuredMessage { get; }

    public ErrorType Type { get; }

    public static Error Failure(string code, string structuredMessage, params string[]? args) =>
        new(code, structuredMessage, ErrorType.Failure, args);

    public static Error NotFound(string code, string structuredMessage, params string[]? args) =>
        new(code, structuredMessage, ErrorType.NotFound, args);

    public static Error Problem(string code, string structuredMessage, params string[]? args) =>
        new(code, structuredMessage, ErrorType.Problem, args);

    public static Error Conflict(string code, string structuredMessage, params string[]? args) =>
        new(code, structuredMessage, ErrorType.Conflict, args);
}