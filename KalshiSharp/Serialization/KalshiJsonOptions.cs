using System.Text.Json;
using System.Text.Json.Serialization;
using KalshiSharp.Serialization.Converters;

namespace KalshiSharp.Serialization;

/// <summary>
/// Provides pre-configured <see cref="JsonSerializerOptions"/> for Kalshi API serialization.
/// </summary>
public static class KalshiJsonOptions
{
    private static JsonSerializerOptions? _options;

    /// <summary>
    /// Gets the singleton <see cref="JsonSerializerOptions"/> configured for Kalshi API.
    /// </summary>
    public static JsonSerializerOptions Default => _options ??= CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        options.Converters.Add(new DecimalStringConverter());
        options.Converters.Add(new NullableDecimalStringConverter());
        options.Converters.Add(new UnixTimestampConverter());
        options.Converters.Add(new NullableUnixTimestampConverter());
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));

        return options;
    }
}
