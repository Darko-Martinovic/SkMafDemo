namespace SkMafDemo.Core.Domain;

public sealed class OrderRepository
{
    private readonly Dictionary<string, Order> _orders;
    private readonly Func<DateOnly> _today;

    public OrderRepository() : this(SeedOrders(), () => DateOnly.FromDateTime(DateTime.UtcNow)) { }

    public OrderRepository(IEnumerable<Order> seed, Func<DateOnly> today)
    {
        _orders = seed.ToDictionary(o => o.OrderId, StringComparer.OrdinalIgnoreCase);
        _today = today;
    }

    public DateOnly Today() => _today();

    public Order? Find(string orderId) =>
        _orders.TryGetValue(orderId, out var order) ? order : null;

    public IReadOnlyCollection<Order> All() => _orders.Values;

    // Seeded so the demos have something to talk about. ORD-10432 is deliberately late
    // so the order-status tool calls in items #2/#8/#12 surface an interesting result.
    private static IEnumerable<Order> SeedOrders()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        yield return new Order(
            "ORD-10428", "Ada Lovelace", "London, UK",
            today.AddDays(-10), today.AddDays(-2), OrderStatus.Delivered,
            new[]
            {
                new OrderLine("BK-001", "Analytical Engine, vol. 1", 1, 49.95m),
                new OrderLine("BK-002", "Analytical Engine, vol. 2", 1, 49.95m)
            });

        yield return new Order(
            "ORD-10429", "Grace Hopper", "Arlington, VA",
            today.AddDays(-6), today.AddDays(2), OrderStatus.Shipped,
            new[]
            {
                new OrderLine("HW-014", "COBOL compiler tape", 2, 199.00m)
            });

        yield return new Order(
            "ORD-10432", "Alan Turing", "Manchester, UK",
            today.AddDays(-9), today.AddDays(-3), OrderStatus.Shipped,
            new[]
            {
                new OrderLine("HW-007", "Enigma rotor set", 1, 850.00m),
                new OrderLine("BK-019", "On Computable Numbers (reprint)", 3, 24.50m)
            });

        yield return new Order(
            "ORD-10440", "Margaret Hamilton", "Cambridge, MA",
            today.AddDays(-2), today.AddDays(5), OrderStatus.Picking,
            new[]
            {
                new OrderLine("HW-022", "Rope-core memory module", 4, 320.00m)
            });

        yield return new Order(
            "ORD-10441", "Donald Knuth", "Palo Alto, CA",
            today, today.AddDays(7), OrderStatus.Placed,
            new[]
            {
                new OrderLine("BK-033", "TAOCP boxed set", 1, 240.00m),
                new OrderLine("MISC-1", "Personalised cheque", 1, 2.56m)
            });
    }
}
