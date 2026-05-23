using FluentAssertions;
using SkMafDemo.Core.Domain;
using SkMafDemo.Core.Tools;

namespace SkMafDemo.Core.Tests;

public class OrderToolsTests
{
    private static OrderTools NewTools() => new(new OrderRepository());

    [Fact]
    public void GetOrderStatus_returns_not_found_for_unknown_id()
    {
        NewTools().GetOrderStatus("ORD-XXXX").Should().Contain("not found");
    }

    [Fact]
    public void GetOrderStatus_flags_ord_10432_as_late()
    {
        var result = NewTools().GetOrderStatus("ORD-10432");
        result.Should().Contain("ORD-10432").And.Contain("late");
    }

    [Fact]
    public void GetOrderStatus_reports_delivered_for_completed_orders()
    {
        NewTools().GetOrderStatus("ORD-10428").Should().Contain("delivered");
    }

    [Fact]
    public void CalculateOrderTotal_sums_line_items()
    {
        // ORD-10432: 1 * 850.00 + 3 * 24.50 = 923.50
        NewTools().CalculateOrderTotal("ORD-10432").Should().Contain("923.50");
    }

    [Fact]
    public void CalculateOrderTotal_returns_not_found_for_unknown_id()
    {
        NewTools().CalculateOrderTotal("ORD-XXXX").Should().Contain("not found");
    }
}
