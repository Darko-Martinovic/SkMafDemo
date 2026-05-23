#pragma warning disable SKEXP0110 // ChatCompletionAgent is marked experimental in SK 1.76.
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SkMafDemo.Console.Hosting;
using SkMafDemo.Console.Menu;
using SkMafDemo.Console.Ui;
using SkMafDemo.Core.Abstractions;
using SkMafDemo.Core.Tools;
using SkMafDemo.SemanticKernel;

namespace SkMafDemo.Console.Demos.Sk;

// What it shows: wrap function-calling in SK's higher-level ChatCompletionAgent
// abstraction. The agent owns instructions + the kernel (and its plugins) + the
// invocation settings; callers just push messages at it.
internal sealed class Sk04_SingleAgentDemo : DemoBase
{
    private readonly OrderTools _orderTools;

    public override int Number => 4;
    public override string Section => Sections.SemanticKernel;
    public override string Title => "SK: Single ChatCompletionAgent with a plugin";
    protected override string Subtitle => "Agent owns the kernel + plugins + instructions; you just send messages.";

    protected override string OfflineExplanation =>
        "Would construct a ChatCompletionAgent named 'OrderAssistant' with OrderAssistantInstructions, " +
        "attach the OrderTools plugin, and call InvokeAsync against a ChatHistory containing the user " +
        "question. The agent's kernel handles function-calling internally.";

    public Sk04_SingleAgentDemo(ChatClientBundle bundle, OrderTools orderTools) : base(bundle)
    {
        _orderTools = orderTools;
    }

    protected override async Task RunOnlineAsync(CancellationToken ct)
    {
        var kernel = Bundle.CreateKernel();
        kernel.Plugins.Add(SkKernelFactory.BuildOrderPlugin(_orderTools));
        kernel.FunctionInvocationFilters.Add(new SkFunctionTraceFilter());

        var agent = new ChatCompletionAgent
        {
            Name = "OrderAssistant",
            Instructions = Prompts.OrderAssistantInstructions,
            Kernel = kernel,
            Arguments = new KernelArguments(new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            })
        };

        var history = new ChatHistory();
        history.AddUserMessage("Is order ORD-10432 going to make it to its customer on time?");
        ConsoleUi.Trace("agent.InvokeAsync(history)");

        await foreach (var message in agent.InvokeAsync(history, cancellationToken: ct))
        {
            // ChatCompletionAgent in SK 1.76 yields AgentResponseItem<ChatMessageContent>.
            // The inner ChatMessageContent has .Role and .Content.
            System.Console.WriteLine($"Agent ({message.Message.Role}): {message.Message.Content}");
        }
    }
}
