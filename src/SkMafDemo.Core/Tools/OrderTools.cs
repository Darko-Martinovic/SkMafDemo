using System.ComponentModel;
using System.Globalization;
using SkMafDemo.Core.Domain;

namespace SkMafDemo.Core.Tools;

// Plain C# tool methods. The [Description] attributes are NOT cosmetic — both SK
// (via [KernelFunction]) and MAF (via AIFunctionFactory) read them and pass them
// to the model as the tool/parameter schema. Re-use these methods in both adapters
// so the business logic is written exactly once.
public sealed class OrderTools
{
    private readonly OrderRepository _orders;

    public OrderTools(OrderRepository orders) => _orders = orders;

    [Description("Looks up an order by its ID and returns its current status, destination, " +
                 "promised delivery date, and whether it is past its promised date.")]
    public string GetOrderStatus(
        [Description("Order identifier, e.g. ORD-10432")] string orderId)
    {
        var order = _orders.Find(orderId);
        if (order is null) return $"Order {orderId} not found.";

        var today = _orders.Today();
        var daysLate = today.DayNumber - order.PromisedBy.DayNumber;
        var lateness = order.Status switch
        {
            OrderStatus.Delivered => "delivered",
            OrderStatus.Cancelled => "cancelled",
            _ when daysLate > 0 => $"running {daysLate} day(s) late",
            _ when daysLate == 0 => "due today",
            _ => $"on track ({-daysLate} day(s) ahead of promise)"
        };

        return string.Create(CultureInfo.InvariantCulture,
            $"Order {order.OrderId} for {order.CustomerName} to {order.Destination}: " +
            $"status={order.Status}, promised={order.PromisedBy:yyyy-MM-dd}, {lateness}.");
    }

    [Description("Calculates the total amount due for an order (sum of all line items).")]
    public string CalculateOrderTotal(
        [Description("Order identifier, e.g. ORD-10432")] string orderId)
    {
        var order = _orders.Find(orderId);
        if (order is null) return $"Order {orderId} not found.";
        var total = order.Lines.Sum(l => l.LineTotal);
        return string.Create(CultureInfo.InvariantCulture,
            $"Order {order.OrderId} total: {total:0.00} ({order.Lines.Count} line(s)).");
    }
}
