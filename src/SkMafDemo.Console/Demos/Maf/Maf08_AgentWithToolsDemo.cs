using SkMafDemo.AgentFramework;
using SkMafDemo.Console.Hosting;
using SkMafDemo.Console.Menu;
using SkMafDemo.Console.Ui;
using SkMafDemo.Core.Abstractions;
using SkMafDemo.Core.Tools;

namespace SkMafDemo.Console.Demos.Maf;

// What it shows: MAF tool calling. The SAME Core methods used by SK #2 are wrapped
// via AIFunctionFactory.Create and handed to the agent's tools list. The agent
// decides when to call them — MAF runs the tool loop internally and returns a
// response whose Messages contain the FunctionCallContent/FunctionResultContent
// pairs we trace here.
internal sealed class Maf08_AgentWithToolsDemo : DemoBase
{
    private readonly OrderTools _orderTools;
    private readonly ShippingTools _shippingTools;

    public override int Number => 8;
    public override string Section => Sections.AgentFramework;
    public override string Title => "MAF: AIAgent with tools (AIFunctionFactory)";
    protected override string Subtitle => "Same Core methods as SK #2 — surface the tool calls.";

    protected override string OfflineExplanation =>
        "Would wrap OrderTools.GetOrderStatus, OrderTools.CalculateOrderTotal, and " +
        "ShippingTools.GetShippingEstimate with AIFunctionFactory.Create, pass them to " +
        "AsAIAgent(tools: ...), and call RunAsync against the same ORD-10432 prompt as SK #2.";

    public Maf08_AgentWithToolsDemo(ChatClientBundle bundle, OrderTools orderTools, ShippingTools shippingTools) : base(bundle)
    {
        _orderTools = orderTools;
        _shippingTools = shippingTools;
    }

    protected override async Task RunOnlineAsync(CancellationToken ct) =>
        await RunCoreAsync(Bundle, _orderTools, _shippingTools, Prompts.OrderTrackingTask, ct);

    // Public helper so demo #12 can invoke the SAME tool-calling path against the
    // same prompt and compare transcripts. The body lives here, not duplicated.
    public static async Task RunCoreAsync(
        ChatClientBundle bundle,
        OrderTools orderTools,
        ShippingTools shippingTools,
        string userPrompt,
        CancellationToken ct)
    {
        if (bundle.ChatClient is null)
            throw new InvalidOperationException("ChatClient is required for MAF demos.");

        var tools = MafAgentFactory.BuildOrderTools(orderTools)
            .Concat(MafAgentFactory.BuildShippingTools(shippingTools))
            .ToList();

        var agent = MafAgentFactory.CreateAgent(
            bundle.ChatClient,
            instructions: Prompts.OrderAssistantInstructions,
            name: "OrderAssistant",
            tools: tools);

        ConsoleUi.Trace($"User: {userPrompt}");
        var response = await agent.RunAsync(userPrompt, cancellationToken: ct);

        MafResponseTracer.Trace(response);
        System.Console.WriteLine();
        System.Console.WriteLine("Agent: " + response.Text);
    }
}
