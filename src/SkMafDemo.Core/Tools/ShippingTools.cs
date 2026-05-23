using System.ComponentModel;
using System.Globalization;

namespace SkMafDemo.Core.Tools;

public sealed class ShippingTools
{
    [Description("Estimates the shipping cost in USD for a parcel of a given weight to a given destination. " +
                 "Uses a simple deterministic formula so results are predictable in demos.")]
    public string GetShippingEstimate(
        [Description("Destination city or 'city, country'")] string destination,
        [Description("Parcel weight in kilograms")] double weightKg)
    {
        // Deterministic — the demos are offline-safe; we never want a random number here.
        var zoneCharge = destination.Contains(',') ? 18.50 : 9.50;
        var weightCharge = Math.Round(weightKg * 4.20, 2);
        var total = Math.Round(zoneCharge + weightCharge, 2);
        return string.Create(CultureInfo.InvariantCulture,
            $"Estimated shipping to {destination} for {weightKg:0.##} kg: ${total:0.00} " +
            $"(zone ${zoneCharge:0.00} + weight ${weightCharge:0.00}).");
    }
}
