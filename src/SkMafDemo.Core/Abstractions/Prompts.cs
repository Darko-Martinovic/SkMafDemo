namespace SkMafDemo.Core.Abstractions;

// Centralised prompt and instruction text. Lives in Core (no framework refs) so
// the SK and MAF adapters can quote the SAME strings — keeping the side-by-side
// comparison honest.
public static class Prompts
{
    public const string OrderAssistantInstructions =
        "You are a logistics assistant for an online retailer. " +
        "When the user mentions an order, call GetOrderStatus to look it up. " +
        "If they ask about shipping or weather, use the appropriate tool. " +
        "Always cite the order id you looked up and quote the status verbatim. " +
        "Keep answers under three sentences.";

    public const string OrderTrackingTask =
        "Please track order ORD-10432 and tell me whether it is running late. " +
        "If it is late, also estimate shipping to its destination for a 2 kg package.";

    public const string SupportTicketSample =
        "Hi — I ordered an Enigma rotor set nine days ago and the tracking says it's still " +
        "in transit. It was supposed to arrive three days ago. I'm getting nervous because " +
        "this is a gift for my supervisor's retirement on Friday and I really need it. " +
        "Could someone please check ORD-10432 and let me know what's happening? Thanks.";

    public const string SummariseTicketPromptTemplate =
        "Summarise the following customer support ticket in one sentence, " +
        "then list the customer's top concern as a single bullet.\n\nTicket:\n{{$ticket}}";

    public const string ResearcherInstructions =
        "You are a research analyst. Given a topic, produce 3–5 short bullet points " +
        "covering the most important facts. Do not write prose.";

    public const string WriterInstructions =
        "You are a technical writer. Take the analyst's bullets and turn them into a " +
        "single, tight paragraph aimed at a senior engineer. No marketing language.";

    public const string EditorInstructions =
        "You are an editor. If the writer's paragraph is good, reply with exactly: APPROVED. " +
        "Otherwise reply with a one-line critique and suggested rewrite. Be terse.";

    public const string CoordinatorInstructions =
        "You coordinate a team. When asked a research question, delegate to the Researcher tool, " +
        "then return its findings to the user. Do not invent facts yourself.";

    // For the RAG demo (#6).
    public const string RagAssistantInstructions =
        "Answer the question using ONLY the provided context snippets. " +
        "If the context does not contain the answer, say so plainly.";
}
