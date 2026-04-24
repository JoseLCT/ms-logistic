using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MsLogistic.Application.Integration.Serialization;

public class StringOrDoubleConverter : JsonConverter<double> {
	public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		return reader.TokenType switch {
			JsonTokenType.Number => reader.GetDouble(),
			JsonTokenType.String => double.Parse(reader.GetString()!, CultureInfo.InvariantCulture),
			_ => throw new JsonException($"Unexpected token {reader.TokenType} when parsing double")
		};
	}

	public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options) {
		writer.WriteNumberValue(value);
	}
}
