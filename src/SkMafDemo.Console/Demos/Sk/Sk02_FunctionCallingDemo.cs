using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SkMafDemo.Console.Hosting;
using SkMafDemo.Console.Menu;
using SkMafDemo.Console.Ui;
using SkMafDemo.Core.Abstractions;
using SkMafDemo.Core.Tools;
using SkMafDemo.SemanticKernel;

namespace SkMafDemo.Console.Demos.Sk;

// What it shows: SK's native function-calling loop. The model is told it has
// tools available; SK exposes them as JSON-schema'd functions; FunctionChoiceBehavior.Auto
// lets the model decide when to call one. The tool trace makes the loop visible.
//
// Pair this with demo #8 (MAF) to compare ceremony: same Core method, two adapters.
internal sealed class Sk02_FunctionCallingDemo : DemoBase
{
    private readonly OrderTools _orderTools;
    private readonly ShippingTools _shippingTools;

    public override int Number => 2;
    public override string Section => Sections.SemanticKernel;
    public override string Title => "SK: Native function calling (FunctionChoiceBehavior.Auto)";
    protected override string Subtitle => "Same Core methods as MAF #8 — surface the tool calls.";

    protected override string OfflineExplanation =>
        "Would build a Kernel, attach the OrderTools and ShippingTools plugins (wrappers over " +
        "the Core methods), set FunctionChoiceBehavior.Auto, and ask the model to track ORD-10432 " +
        "and estimate shipping. SK would invoke GetOrderStatus and (if late) GetShippingEstimate.";

    public Sk02_FunctionCallingDemo(ChatClientBundle bundle, OrderTools orderTools, ShippingTools shippingTools)
        : base(bundle)
    {
        _orderTools = orderTools;
        _shippingTools = shippingTools;
    }

    protected override async Task RunOnlineAsync(CancellationToken ct) =>
        await RunCoreAsync(Bundle, _orderTools, _shippingTools, Prompts.OrderTrackingTask, ct);

    // Public helper so demo #12 can invoke the SAME function-calling path against
    // the same prompt and compare transcripts. The body lives here, not duplicated.
    public static async Task RunCoreAsync(
        ChatClientBundle bundle,
        OrderTools orderTools,
        ShippingTools shippingTools,
        string userPrompt,
        CancellationToken ct)
    {
        var kernel = bundle.CreateKernel();
        kernel.Plugins.Add(SkKernelFactory.BuildOrderPlugin(orderTools));
        kernel.Plugins.Add(SkKernelFactory.BuildShippingPlugin(shippingTools));
        kernel.FunctionInvocationFilters.Add(new SkFunctionTraceFilter());

        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory(Prompts.OrderAssistantInstructions);
        history.AddUserMessage(userPrompt);

        var settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        ConsoleUi.Trace($"User: {userPrompt}");
        var reply = await chat.GetChatMessageContentAsync(history, settings, kernel, ct);
        System.Console.WriteLine();
        System.Console.WriteLine("Model: " + reply.Content);
    }
}
