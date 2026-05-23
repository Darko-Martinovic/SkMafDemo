using FluentAssertions;
using SkMafDemo.Core.Tools;

namespace SkMafDemo.Core.Tests;

public class ShippingToolsTests
{
    [Fact]
    public void GetShippingEstimate_is_deterministic_for_a_given_input()
    {
        var tools = new ShippingTools();
        var a = tools.GetShippingEstimate("Manchester, UK", 2.0);
        var b = tools.GetShippingEstimate("Manchester, UK", 2.0);
        a.Should().Be(b);
    }

    [Fact]
    public void GetShippingEstimate_uses_higher_zone_when_destination_includes_country()
    {
        var tools = new ShippingTools();
        var withCountry = tools.GetShippingEstimate("Manchester, UK", 0.0);
        var withoutCountry = tools.GetShippingEstimate("Manchester", 0.0);
        // Zone charge $18.50 vs $9.50 at zero weight — the difference must surface.
        withCountry.Should().Contain("18.50");
        withoutCountry.Should().Contain("9.50");
    }

    [Fact]
    public void GetShippingEstimate_scales_with_weight()
    {
        var tools = new ShippingTools();
        // 10 kg * $4.20 = $42.00 weight component.
        tools.GetShippingEstimate("Manchester, UK", 10.0).Should().Contain("42.00");
    }
}
