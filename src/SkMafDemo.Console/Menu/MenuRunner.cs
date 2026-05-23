using Microsoft.Extensions.Hosting;
using SkMafDemo.Console.Hosting;
using SkMafDemo.Console.Ui;
using SkMafDemo.Core.Abstractions;

namespace SkMafDemo.Console.Menu;

internal sealed class MenuRunner : IHostedService
{
    private readonly IReadOnlyList<IDemo> _demos;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ChatClientBundle _bundle;
    private CancellationTokenSource? _runCts;
    private Task? _runTask;

    public MenuRunner(IEnumerable<IDemo> demos, IHostApplicationLifetime lifetime, ChatClientBundle bundle)
    {
        _demos = demos.OrderBy(d => d.Number).ToArray();
        _lifetime = lifetime;
        _bundle = bundle;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _runCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _runTask = Task.Run(() => LoopAsync(_runCts.Token), _runCts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _runCts?.Cancel();
        if (_runTask is not null)
        {
            try { await _runTask.WaitAsync(cancellationToken); }
            catch (OperationCanceledException) { /* expected during shutdown */ }
        }
    }

    private async Task LoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            PrintMenu();
            System.Console.Write("Choice: ");
            var input = System.Console.ReadLine();
            if (input is null) break;                  // stdin EOF — exit cleanly
            if (string.IsNullOrWhiteSpace(input)) continue;
            if (!int.TryParse(input.Trim(), out var choice))
            {
                ConsoleUi.Warn("Please enter a number.");
                continue;
            }
            if (choice == 0) break;

            var demo = _demos.FirstOrDefault(d => d.Number == choice);
            if (demo is null)
            {
                ConsoleUi.Warn($"No demo numbered {choice}.");
                continue;
            }

            try
            {
                await demo.RunAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // Brief: "never dump a raw stack trace at the user; explain what
                // configuration is likely missing." We surface the message but not
                // the trace, with a hint about provider config when applicable.
                ConsoleUi.Error($"Demo failed: {ex.GetType().Name}: {ex.Message}");
                if (!_bundle.Config.IsConfigured)
                {
                    ConsoleUi.Warn("Hint: no model provider is configured. " +
                                   "See README for `dotnet user-secrets` setup commands.");
                }
                else
                {
                    ConsoleUi.Warn("Hint: check the provider endpoint and credentials, " +
                                   "and that the model/deployment name exists.");
                }
            }

            ConsoleUi.PauseForMenu();
        }

        _lifetime.StopApplication();
    }

    private void PrintMenu()
    {
        // Console.Clear() throws IOException when stdout is redirected (CI, piped input).
        // Swallow that case — visual clearing is a nice-to-have for interactive use only.
        try { System.Console.Clear(); } catch (IOException) { }
        var prev = System.Console.ForegroundColor;
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine("=== SK vs MAF Demo ===");
        System.Console.ForegroundColor = prev;
        PrintProviderLine();
        System.Console.WriteLine();

        foreach (var grouping in _demos.GroupBy(d => d.Section))
        {
            System.Console.ForegroundColor = ConsoleColor.DarkCyan;
            System.Console.WriteLine($"--- {grouping.Key} ---");
            System.Console.ForegroundColor = prev;
            foreach (var demo in grouping.OrderBy(d => d.Number))
            {
                System.Console.WriteLine($"{demo.Number,3}) {demo.Title}");
            }
            System.Console.WriteLine();
        }

        System.Console.WriteLine("  0) Exit");
        System.Console.WriteLine();
    }

    private void PrintProviderLine()
    {
        var cfg = _bundle.Config;
        var statusColour = cfg.IsConfigured ? ConsoleColor.Green : ConsoleColor.DarkYellow;
        var prev = System.Console.ForegroundColor;
        System.Console.ForegroundColor = statusColour;
        if (cfg.IsConfigured)
        {
            System.Console.WriteLine($"Active provider: {cfg.Provider} | Model: {cfg.Model} | Endpoint: {cfg.Endpoint}");
        }
        else if (cfg.Provider == ChatProvider.None)
        {
            System.Console.WriteLine("Active provider: (none) — set Ai:Provider to OpenAI, AzureOpenAI, or Ollama. Demos will explain instead of calling a model.");
        }
        else
        {
            System.Console.WriteLine($"Active provider: {cfg.Provider} (incomplete config) — demos will explain instead of calling a model.");
        }
        System.Console.ForegroundColor = prev;
    }
}
