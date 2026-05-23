using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SkMafDemo.Console.Hosting;
using SkMafDemo.Console.Menu;
using SkMafDemo.Console.Ui;

namespace SkMafDemo.Console.Demos.Sk;

// What it shows: the smallest possible SK program. Build a Kernel, get the
// IChatCompletionService it registered, send one message, print the reply. No
// plugins, no tools, no agents — just the plain chat-completion contract.
internal sealed class Sk01_PlainChatDemo : DemoBase
{
    public override int Number => 1;
    public override string Section => Sections.SemanticKernel;
    public override string Title => "SK: Plain chat completion (no tools)";
    protected override string Subtitle => "Streamed where supported. The smallest SK program.";

    protected override string OfflineExplanation =>
        "Would call IChatCompletionService.GetStreamingChatMessageContentsAsync with the user " +
        "prompt 'Give me a one-sentence elevator pitch for Semantic Kernel.' and stream the reply.";

    public Sk01_PlainChatDemo(ChatClientBundle bundle) : base(bundle) { }

    protected override async Task RunOnlineAsync(CancellationToken ct)
    {
        var kernel = Bundle.CreateKernel();
        var chat = kernel.GetRequiredService<IChatCompletionService>();

        var history = new ChatHistory();
        history.AddUserMessage("Give me a one-sentence elevator pitch for Semantic Kernel.");

        ConsoleUi.Trace("kernel.GetRequiredService<IChatCompletionService>().GetStreamingChatMessageContentsAsync(...)");
        System.Console.Write("Model: ");
        await foreach (var chunk in chat.GetStreamingChatMessageContentsAsync(history, kernel: kernel, cancellationToken: ct))
        {
            System.Console.Write(chunk.Content);
        }
        System.Console.WriteLine();
    }
}
