using SkMafDemo.Console.Hosting;
using SkMafDemo.Console.Ui;

namespace SkMafDemo.Console.Menu;

// Base for "real" demos. Centralises:
//   - the header line,
//   - the "no model configured" fallback (each demo supplies its own explanation),
//   - the discipline of never crashing the menu — RunAsync only throws on TRULY
//     unexpected failures; expected provider/auth errors are caught and explained.
internal abstract class DemoBase : IDemo
{
    public abstract int Number { get; }
    public abstract string Section { get; }
    public abstract string Title { get; }
    protected abstract string Subtitle { get; }
    protected abstract string OfflineExplanation { get; }
    protected abstract Task RunOnlineAsync(CancellationToken ct);

    protected ChatClientBundle Bundle { get; }
    protected DemoBase(ChatClientBundle bundle) => Bundle = bundle;

    public async Task RunAsync(CancellationToken ct)
    {
        ConsoleUi.Header($"{Number}) {Title}", Subtitle);
        if (!Bundle.Config.IsConfigured)
        {
            ConsoleUi.Warn("No chat-model provider configured — describing the demo instead of running it.");
            ConsoleUi.Info(OfflineExplanation);
            return;
        }
        await RunOnlineAsync(ct);
    }
}
