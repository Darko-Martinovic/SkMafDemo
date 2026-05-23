namespace SkMafDemo.Core.Domain;

public sealed record OrderLine(string Sku, string Description, int Quantity, decimal UnitPrice)
{
    public decimal LineTotal => Quantity * UnitPrice;
}
