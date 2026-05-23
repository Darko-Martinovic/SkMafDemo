namespace SkMafDemo.Core.Domain;

public sealed record KnowledgeSnippet(string Title, string Body);

// Tiny hand-authored corpus used by the SK RAG demo (#6). Small enough that a
// keyword retriever is a perfectly honest fallback when no embedding model is
// configured — the demo prints which retrieval mode it used.
public static class KnowledgeBase
{
    public static readonly IReadOnlyList<KnowledgeSnippet> Snippets = new[]
    {
        new KnowledgeSnippet(
            "Return policy",
            "Customers can return any item within 30 days of delivery for a full refund. " +
            "Items must be in original packaging. Refunds are issued to the original payment method " +
            "within 5 business days of receipt."),
        new KnowledgeSnippet(
            "Shipping zones",
            "Domestic orders ship same day if placed before 2pm. International orders within Europe " +
            "ship next business day; orders to the rest of the world ship within 3 business days. " +
            "We do not currently ship to PO boxes."),
        new KnowledgeSnippet(
            "Late delivery promise",
            "If an order arrives more than 48 hours past its promised delivery date, the customer is " +
            "entitled to a 15% credit on a future order. This must be requested within 14 days."),
        new KnowledgeSnippet(
            "Order cancellation",
            "Orders can be cancelled free of charge until they enter the 'Picking' status. " +
            "Once picking has begun, a $5 restocking fee applies. Shipped orders cannot be cancelled — " +
            "they must be returned per the return policy."),
        new KnowledgeSnippet(
            "Customer support hours",
            "Live chat is staffed 06:00–22:00 UTC, seven days a week. Outside those hours, customers " +
            "should use the support form; replies arrive within four working hours.")
    };
}
