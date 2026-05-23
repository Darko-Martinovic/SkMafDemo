using SkMafDemo.Console.Demos.Maf;
using SkMafDemo.Console.Demos.Sk;
using SkMafDemo.Console.Hosting;
using SkMafDemo.Console.Menu;
using SkMafDemo.Console.Ui;
using SkMafDemo.Core.Abstractions;
using SkMafDemo.Core.Tools;

namespace SkMafDemo.Console.Demos.SideBySide;

// What it shows: the same task, the same Core tools, two adapters. Demo 2 (SK)
// and Demo 8 (MAF) both run their tool-calling loops; we print both transcripts
// and a short comparison of how each framework expressed the work.
internal sealed class SideBySideDemo : DemoBase
{
    private readonly OrderTools _orderTools;
    private readonly ShippingTools _shippingTools;

    public override int Number => 12;
    public override string Section => Sections.SideBySide;
    public override string Title => "SAME task on BOTH frameworks (track ORD-10432 and say if it's late)";
    protected override string Subtitle => "Identical Core tools, identical prompt, two adapters.";

    protected override string OfflineExplanation =>
        "Would run the SK #2 path and the MAF #8 path against the identical prompt " +
        "'" + Prompts.OrderTrackingTask + "' — printing both transcripts and a short " +
        "summary of how the two adapters differed (ceremony, where the tool lived, " +
        "how function-calling was enabled).";

    public SideBySideDemo(ChatClientBundle bundle, OrderTools orderTools, ShippingTools shippingTools) : base(bundle)
    {
        _orderTools = orderTools;
        _shippingTools = shippingTools;
    }

    protected override async Task RunOnlineAsync(CancellationToken ct)
    {
        ConsoleUi.Info("--- Run 1: Semantic Kernel (path of demo #2) ---");
        await Sk02_FunctionCallingDemo.RunCoreAsync(Bundle, _orderTools, _shippingTools, Prompts.OrderTrackingTask, ct);

        System.Console.WriteLine();
        ConsoleUi.Info("--- Run 2: Microsoft Agent Framework (path of demo #8) ---");
        await Maf08_AgentWithToolsDemo.RunCoreAsync(Bundle, _orderTools, _shippingTools, Prompts.OrderTrackingTask, ct);

        System.Console.WriteLine();
        ConsoleUi.Info("--- Diff of approach ---");
        ConsoleUi.Bullet("Both adapters wrap the SAME Core methods (OrderTools.GetOrderStatus / ShippingTools.GetShippingEstimate).");
        ConsoleUi.Bullet("SK: methods are wrapped as KernelFunctions on a KernelPlugin; the Kernel hosts plugins + a chat-completion service.");
        ConsoleUi.Bullet("MAF: methods are wrapped as AIFunctions via AIFunctionFactory.Create and passed directly to AsAIAgent's tools list.");
        ConsoleUi.Bullet("Function-calling is opt-in for SK (FunctionChoiceBehavior.Auto in PromptExecutionSettings); MAF enables it by default when tools are present.");
        ConsoleUi.Bullet("Tool-call observability: SK has IFunctionInvocationFilter; MAF surfaces FunctionCallContent / FunctionResultContent in AgentResponse.Messages.");
        ConsoleUi.Bullet("[Description] attributes on the Core methods drive the schema both frameworks show the model — the descriptions are written ONCE.");
    }
}
