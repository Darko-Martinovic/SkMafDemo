using System.ComponentModel;

namespace SkMafDemo.Core.Tools;

public sealed class WeatherTools
{
    // Canned data so the demos are deterministic and run offline. Real implementations
    // would call an HTTP weather provider — the shape of the tool stays identical.
    private static readonly Dictionary<string, string> _conditions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["london"] = "12 C, light rain, wind 18 km/h",
        ["manchester"] = "10 C, overcast, wind 22 km/h",
        ["palo alto"] = "21 C, sunny, wind 6 km/h",
        ["arlington"] = "17 C, partly cloudy, wind 11 km/h",
        ["cambridge"] = "14 C, drizzle, wind 14 km/h"
    };

    [Description("Returns current weather conditions for a known city. " +
                 "Supports: London, Manchester, Palo Alto, Arlington, Cambridge.")]
    public string GetWeather(
        [Description("City name (case-insensitive)")] string city)
    {
        var key = city.Split(',')[0].Trim();
        return _conditions.TryGetValue(key, out var conditions)
            ? $"Current weather in {key}: {conditions}."
            : $"Weather data is not available for '{city}'. Known cities: {string.Join(", ", _conditions.Keys)}.";
    }
}
