using SkMafDemo.AgentFramework;
using SkMafDemo.Console.Hosting;
using SkMafDemo.Console.Menu;
using SkMafDemo.Console.Ui;

namespace SkMafDemo.Console.Demos.Maf;

// What it shows: MAF's minimal program. A single AIAgent constructed via
// IChatClient.AsAIAgent — no tools, no workflow. Compare to SK demo #1.
internal sealed class Maf07_PlainAgentDemo : DemoBase
{
    public override int Number => 7;
    public override string Section => Sections.AgentFramework;
    public override string Title => "MAF: Single AIAgent, plain run";
    protected override string Subtitle => "chatClient.AsAIAgent(instructions, name) — then RunAsync.";

    protected override string OfflineExplanation =>
        "Would build an AIAgent named 'Assistant' via chatClient.AsAIAgent(...) and call " +
        "RunAsync('Give me a one-sentence elevator pitch for the Microsoft Agent Framework.'). " +
        "The reply text would be on the returned AgentRunResponse.Text.";

    public Maf07_PlainAgentDemo(ChatClientBundle bundle) : base(bundle) { }

    protected override async Task RunOnlineAsync(CancellationToken ct)
    {
        if (Bundle.ChatClient is null)
            throw new InvalidOperationException("ChatClient is required for MAF demos.");

        var agent = MafAgentFactory.CreateAgent(
            Bundle.ChatClient,
            instructions: "You are concise. One sentence answers, no preamble.",
            name: "Assistant");

        ConsoleUi.Trace("agent.RunAsync(prompt)");
        var response = await agent.RunAsync(
            "Give me a one-sentence elevator pitch for the Microsoft Agent Framework.",
            cancellationToken: ct);

        System.Console.WriteLine();
        System.Console.WriteLine("Agent: " + response.Text);
    }
}
