namespace SkMafDemo.Core.Domain;

public sealed record Order(
    string OrderId,
    string CustomerName,
    string Destination,
    DateOnly PlacedOn,
    DateOnly PromisedBy,
    OrderStatus Status,
    IReadOnlyList<OrderLine> Lines);
