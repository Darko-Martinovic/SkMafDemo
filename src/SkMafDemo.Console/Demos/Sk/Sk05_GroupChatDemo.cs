#pragma warning disable SKEXP0001, SKEXP0110 // AgentGroupChat + strategies are experimental in SK 1.76.
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using SkMafDemo.Console.Hosting;
using SkMafDemo.Console.Menu;
using SkMafDemo.Console.Ui;
using SkMafDemo.Core.Abstractions;

namespace SkMafDemo.Console.Demos.Sk;

// What it shows: multi-agent orchestration in SK. Three agents — Researcher → Writer → Editor —
// take turns under a SequentialSelectionStrategy. The chat ends when the Editor says "APPROVED".
// This pattern (named agents + turn-taking strategies) is SK's current lead over MAF.
internal sealed class Sk05_GroupChatDemo : DemoBase
{
    public override int Number => 5;
    public override string Section => Sections.SemanticKernel;
    public override string Title => "SK: Multi-agent AgentGroupChat (Researcher → Writer → Editor)";
    protected override string Subtitle => "Sequential selection + a custom 'editor says APPROVED' termination strategy.";

    protected override string OfflineExplanation =>
        "Would construct three ChatCompletionAgents (Researcher, Writer, Editor), wire them into " +
        "an AgentGroupChat with SequentialSelectionStrategy and an ApprovedTerminationStrategy " +
        "(stops when the Editor's reply is exactly 'APPROVED'), then push a topic in and stream " +
        "the turn-by-turn transcript.";

    public Sk05_GroupChatDemo(ChatClientBundle bundle) : base(bundle) { }

    protected override async Task RunOnlineAsync(CancellationToken ct)
    {
        var kernel = Bundle.CreateKernel();

        var researcher = new ChatCompletionAgent
        {
            Name = "Researcher",
            Instructions = Prompts.ResearcherInstructions,
            Kernel = kernel
        };
        var writer = new ChatCompletionAgent
        {
            Name = "Writer",
            Instructions = Prompts.WriterInstructions,
            Kernel = kernel
        };
        var editor = new ChatCompletionAgent
        {
            Name = "Editor",
            Instructions = Prompts.EditorInstructions,
            Kernel = kernel
        };

        var chat = new AgentGroupChat(researcher, writer, editor)
        {
            ExecutionSettings = new AgentGroupChatSettings
            {
                SelectionStrategy = new SequentialSelectionStrategy { InitialAgent = researcher },
                TerminationStrategy = new ApprovedTerminationStrategy
                {
                    Agents = new[] { editor },
                    MaximumIterations = 6
                }
            }
        };

        chat.AddChatMessage(new ChatMessageContent(AuthorRole.User,
            "Topic: 'when should a .NET team pick Semantic Kernel over Microsoft Agent Framework?'"));

        ConsoleUi.Trace("AgentGroupChat.InvokeAsync — streaming turns until Editor says APPROVED or 6 turns.");

        await foreach (var message in chat.InvokeAsync(ct))
        {
            System.Console.WriteLine();
            ConsoleUi.Info($"--- {message.AuthorName} ---");
            System.Console.WriteLine(message.Content);
        }

        System.Console.WriteLine();
        ConsoleUi.Info(chat.IsComplete ? "Chat terminated cleanly." : "Chat hit the iteration cap.");
    }

    // Local termination strategy — keeps the demo self-contained. Production code
    // would usually live in its own file, but here it's right where the reader is.
    private sealed class ApprovedTerminationStrategy : TerminationStrategy
    {
        protected override Task<bool> ShouldAgentTerminateAsync(
            Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
        {
            var last = history.LastOrDefault(m => m.AuthorName == agent.Name)?.Content ?? string.Empty;
            return Task.FromResult(last.Contains("APPROVED", StringComparison.Ordinal));
        }
    }
}
