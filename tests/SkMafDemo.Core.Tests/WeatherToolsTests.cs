using FluentAssertions;
using SkMafDemo.Core.Tools;

namespace SkMafDemo.Core.Tests;

public class WeatherToolsTests
{
    [Fact]
    public void GetWeather_returns_canned_data_for_known_city()
    {
        new WeatherTools().GetWeather("Manchester").Should().Contain("Manchester");
    }

    [Fact]
    public void GetWeather_is_case_insensitive()
    {
        // The lookup is case-insensitive (matches the dictionary entry), even though
        // the echoed text mirrors the caller's casing.
        new WeatherTools().GetWeather("LONDON").Should().Contain("12 C, light rain");
    }

    [Fact]
    public void GetWeather_handles_city_country_format()
    {
        new WeatherTools().GetWeather("Manchester, UK").Should().Contain("Manchester");
    }

    [Fact]
    public void GetWeather_explains_when_city_is_unknown()
    {
        new WeatherTools().GetWeather("Atlantis").Should().Contain("not available");
    }
}
