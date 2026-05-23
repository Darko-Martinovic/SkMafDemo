using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SkMafDemo.Console.Hosting;
using SkMafDemo.Console.Menu;
using SkMafDemo.Console.Ui;
using SkMafDemo.Core.Abstractions;
using SkMafDemo.Core.Domain;

namespace SkMafDemo.Console.Demos.Sk;

// What it shows: a minimal retrieval-augmented generation loop. The corpus is the
// hand-authored KnowledgeBase snippets in Core. With no embedding model configured
// we use a keyword retriever and SAY SO — the brief insists we be honest about the
// fallback. Top hits are injected into the prompt; the model is told to answer
// only from that context.
internal sealed class Sk06_RagDemo : DemoBase
{
    public override int Number => 6;
    public override string Section => Sections.SemanticKernel;
    public override string Title => "SK: Memory / RAG over a tiny in-memory document set";
    protected override string Subtitle => "Retriever → context-stuffed prompt → grounded answer. Keyword-fallback retrieval.";

    protected override string OfflineExplanation =>
        "Would keyword-rank the KnowledgeBase snippets against the question 'what happens if my " +
        "order is late?', stuff the top 2 hits into the prompt as context, and ask the model to " +
        "answer using only that context. The retrieval mode (keyword vs embedding) is printed.";

    public Sk06_RagDemo(ChatClientBundle bundle) : base(bundle) { }

    protected override async Task RunOnlineAsync(CancellationToken ct)
    {
        var question = "If my order is more than two days late, am I entitled to anything?";

        ConsoleUi.Info("Retrieval mode: KEYWORD overlap (no embedding model is configured).");
        var hits = TopByKeyword(question, 2).ToList();
        foreach (var hit in hits) ConsoleUi.Trace($"retrieved: {hit.Title}");

        var contextBlock = string.Join("\n\n",
            hits.Select(h => $"# {h.Title}\n{h.Body}"));

        var kernel = Bundle.CreateKernel();
        var chat = kernel.GetRequiredService<IChatCompletionService>();

        var history = new ChatHistory(Prompts.RagAssistantInstructions);
        history.AddUserMessage($"Context:\n{contextBlock}\n\nQuestion: {question}");

        System.Console.WriteLine();
        System.Console.WriteLine("Question: " + question);
        System.Console.Write("Model: ");
        await foreach (var chunk in chat.GetStreamingChatMessageContentsAsync(history, kernel: kernel, cancellationToken: ct))
        {
            System.Console.Write(chunk.Content);
        }
        System.Console.WriteLine();
    }

    private static IEnumerable<KnowledgeSnippet> TopByKeyword(string query, int top)
    {
        var queryTokens = Tokenise(query).ToHashSet();
        return KnowledgeBase.Snippets
            .Select(s => (snippet: s, score: Tokenise(s.Title + " " + s.Body).Count(queryTokens.Contains)))
            .Where(t => t.score > 0)
            .OrderByDescending(t => t.score)
            .Take(top)
            .Select(t => t.snippet);
    }

    private static IEnumerable<string> Tokenise(string s) =>
        s.ToLowerInvariant()
         .Split(new[] { ' ', '.', ',', ';', ':', '?', '!', '\n', '\r', '\t', '(', ')', '\'' },
                StringSplitOptions.RemoveEmptyEntries)
         .Where(t => t.Length > 3);   // drop trivially-common short words
}
