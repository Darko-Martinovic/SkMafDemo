using System.ComponentModel;
using Microsoft.Extensions.AI;
using SkMafDemo.AgentFramework;
using SkMafDemo.Console.Hosting;
using SkMafDemo.Console.Menu;
using SkMafDemo.Console.Ui;
using SkMafDemo.Core.Abstractions;

namespace SkMafDemo.Console.Demos.Maf;

// What it shows: MAF's "agent as a tool" pattern. The Researcher is itself an
// AIAgent; we expose its RunAsync as an AIFunction via AIFunctionFactory and
// hand it to the Coordinator agent in its tools list. The Coordinator decides
// when to delegate. [Description] on the wrapper drives the schema the model sees.
internal sealed class Maf09_AgentsAsToolsDemo : DemoBase
{
    public override int Number => 9;
    public override string Section => Sections.AgentFramework;
    public override string Title => "MAF: Agents-as-tools (Coordinator delegates to Researcher)";
    protected override string Subtitle => "Researcher.RunAsync wrapped as an AIFunction the Coordinator can call.";

    protected override string OfflineExplanation =>
        "Would create two agents: a Researcher (ResearcherInstructions) and a Coordinator " +
        "(CoordinatorInstructions). A wrapper method 'Research(topic)' calls Researcher.RunAsync " +
        "internally and is exposed to the Coordinator via AIFunctionFactory.Create. When asked " +
        "a research question, the Coordinator chooses to call that tool.";

    public Maf09_AgentsAsToolsDemo(ChatClientBundle bundle) : base(bundle) { }

    protected override async Task RunOnlineAsync(CancellationToken ct)
    {
        if (Bundle.ChatClient is null)
            throw new InvalidOperationException("ChatClient is required for MAF demos.");

        var researcher = MafAgentFactory.CreateAgent(
            Bundle.ChatClient,
            instructions: Prompts.ResearcherInstructions,
            name: "Researcher");

        // Local function whose [Description] attributes give the Coordinator the schema
        // it shows the model. Body just delegates to the Researcher agent.
        [Description("Researches a topic and returns 3–5 short bullet points of key facts.")]
        async Task<string> Research(
            [Description("The topic to research, e.g. 'distributed tracing in .NET'")] string topic,
            CancellationToken ctInner)
        {
            ConsoleUi.Trace($"Coordinator → Researcher.RunAsync(\"{topic}\")");
            var result = await researcher.RunAsync(topic, cancellationToken: ctInner);
            return result.Text;
        }

        var researchTool = AIFunctionFactory.Create((Func<string, CancellationToken, Task<string>>)Research);
        var coordinator = MafAgentFactory.CreateAgent(
            Bundle.ChatClient,
            instructions: Prompts.CoordinatorInstructions,
            name: "Coordinator",
            tools: new[] { (AITool)researchTool });

        var question = "What are the key trade-offs of using gRPC vs HTTP/JSON for inter-service communication?";
        ConsoleUi.Trace($"User → Coordinator: {question}");

        var response = await coordinator.RunAsync(question, cancellationToken: ct);
        MafResponseTracer.Trace(response);

        System.Console.WriteLine();
        System.Console.WriteLine("Coordinator: " + response.Text);
    }
}
