using SkMafDemo.Console.Ui;

namespace SkMafDemo.Console.Menu;

// Placeholder for menu items that have not yet been wired to a real adapter.
// The brief insists that "the menu still loads" even when nothing is configured —
// this keeps the menu coherent during incremental delivery and serves as the
// "no model configured" fallback for unimplemented entries.
internal sealed class StubDemo : IDemo
{
    public int Number { get; }
    public string Section { get; }
    public string Title { get; }
    private readonly string _explanation;

    public StubDemo(int number, string section, string title, string explanation)
    {
        Number = number;
        Section = section;
        Title = title;
        _explanation = explanation;
    }

    public Task RunAsync(CancellationToken ct)
    {
        ConsoleUi.Header($"{Number}) {Title}", "(stub — not yet wired in this build)");
        ConsoleUi.Info(_explanation);
        return Task.CompletedTask;
    }
}
