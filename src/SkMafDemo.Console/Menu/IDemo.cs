namespace SkMafDemo.Console.Menu;

// One menu entry. Number drives display order; Section groups SK vs MAF vs Side-by-side.
public interface IDemo
{
    int Number { get; }
    string Section { get; }
    string Title { get; }
    Task RunAsync(CancellationToken ct);
}
