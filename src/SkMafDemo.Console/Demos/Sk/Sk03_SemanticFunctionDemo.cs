using Microsoft.SemanticKernel;
using SkMafDemo.Console.Hosting;
using SkMafDemo.Console.Menu;
using SkMafDemo.Console.Ui;
using SkMafDemo.Core.Abstractions;

namespace SkMafDemo.Console.Demos.Sk;

// What it shows: SK's "prompt-as-function" pattern. The prompt template is
// turned into an invokable KernelFunction with named arguments. This is SK's
// signature concept — MAF doesn't have a direct equivalent (instructions live
// on the agent instead).
internal sealed class Sk03_SemanticFunctionDemo : DemoBase
{
    public override int Number => 3;
    public override string Section => Sections.SemanticKernel;
    public override string Title => "SK: Semantic (prompt) function — summarise a support ticket";
    protected override string Subtitle => "CreateFunctionFromPrompt — a prompt with named arguments behaves like a method.";

    protected override string OfflineExplanation =>
        "Would call kernel.CreateFunctionFromPrompt with the SummariseTicket template, then " +
        "invoke it with KernelArguments { [\"ticket\"] = sampleSupportTicket }. The result " +
        "would be a one-line summary plus a bullet identifying the customer's top concern.";

    public Sk03_SemanticFunctionDemo(ChatClientBundle bundle) : base(bundle) { }

    protected override async Task RunOnlineAsync(CancellationToken ct)
    {
        var kernel = Bundle.CreateKernel();

        var summarise = kernel.CreateFunctionFromPrompt(
            Prompts.SummariseTicketPromptTemplate,
            functionName: "SummariseTicket",
            description: "Summarises a customer support ticket.");

        ConsoleUi.Trace($"kernel.InvokeAsync(SummariseTicket, ticket=<{Prompts.SupportTicketSample.Length} chars>)");
        var result = await kernel.InvokeAsync(summarise, new KernelArguments { ["ticket"] = Prompts.SupportTicketSample }, ct);

        System.Console.WriteLine();
        System.Console.WriteLine("Model:");
        System.Console.WriteLine(result.GetValue<string>());
    }
}
