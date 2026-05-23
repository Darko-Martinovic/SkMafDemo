using Microsoft.Agents.AI.Workflows;
using SkMafDemo.Console.Menu;
using SkMafDemo.Console.Ui;
using SkMafDemo.Core.Domain;

namespace SkMafDemo.Console.Demos.Maf;

// What it shows: MAF's WorkflowBuilder — explicit executors and edges, with a
// CONDITIONAL branch. This demo does NOT need an LLM — it shows the orchestration
// primitive that's MAF's current lead over SK. We look up an order, then either
// route to the Escalation executor (if late) or the Confirmation executor.
internal sealed class Maf10_WorkflowDemo : IDemo
{
    private readonly OrderRepository _orders;

    public int Number => 10;
    public string Section => Sections.AgentFramework;
    public string Title => "MAF: Graph workflow (explicit nodes/edges, conditional branch)";

    public Maf10_WorkflowDemo(OrderRepository orders) => _orders = orders;

    public async Task RunAsync(CancellationToken ct)
    {
        ConsoleUi.Header($"{Number}) {Title}",
            "Lookup → (late?) → Escalation | Confirmation. Pure workflow, no LLM needed.");

        // Run twice — once with the deliberately-late ORD-10432, once with the on-track
        // ORD-10440 — so the conditional branch is visible. A MAF Workflow instance is
        // single-use (the runner takes ownership), so we rebuild it each iteration.
        foreach (var orderId in new[] { "ORD-10432", "ORD-10440" })
        {
            var lookup = new OrderLookupExecutor(_orders);
            var escalate = new EscalationExecutor();
            var confirm = new ConfirmationExecutor();

            // The AddEdge<T>(...) condition parameter is Func<T?, bool> in MAF 1.6.2;
            // the framework calls it with non-null messages at runtime but the nullable
            // analysis sees T?, so we pattern-match defensively.
            var workflow = new WorkflowBuilder(lookup)
                .AddEdge<OrderLookupResult>(lookup, escalate, r => r is { IsLate: true }, label: "late")
                .AddEdge<OrderLookupResult>(lookup, confirm, r => r is { IsLate: false }, label: "on-time")
                // WithOutputFrom tells the runtime which executors' YieldOutputAsync calls
                // bubble out as WorkflowOutputEvent. Without it the outputs are swallowed.
                .WithOutputFrom(lookup, escalate, confirm)
                .Build();

            ConsoleUi.Trace($"InProcessExecution.RunStreamingAsync(workflow, \"{orderId}\")");
            await using var run = await InProcessExecution.RunStreamingAsync(workflow, orderId, cancellationToken: ct);
            await foreach (var ev in run.WatchStreamAsync(ct))
            {
                if (ev is WorkflowOutputEvent output)
                {
                    System.Console.WriteLine();
                    ConsoleUi.Info($"Workflow output ({orderId}): {output.Data}");
                }
                else if (ev is ExecutorFailedEvent failed)
                {
                    ConsoleUi.Error($"executor {failed.ExecutorId} failed: {failed.Data?.GetType().Name}: {failed.Data?.Message}");
                }
            }
        }
    }

    private sealed record OrderLookupResult(string OrderId, string StatusLine, bool IsLate);

    // MAF Workflows REQUIRES executors to declare their outgoing message types via
    // [SendsMessage] (for SendMessageAsync) and [YieldsOutput] (for YieldOutputAsync).
    // The runtime rejects messages of un-declared types — declarative wiring is part
    // of the framework's contract, not an optional optimisation.
    [SendsMessage(typeof(OrderLookupResult))]
    [YieldsOutput(typeof(string))]
    private sealed class OrderLookupExecutor : Executor<string>
    {
        private readonly OrderRepository _orders;
        public OrderLookupExecutor(OrderRepository orders)
            : base(id: "lookup", options: null, declareCrossRunShareable: true) => _orders = orders;

        public override async ValueTask HandleAsync(string input, IWorkflowContext context, CancellationToken ct = default)
        {
            var order = _orders.Find(input);
            if (order is null)
            {
                await context.YieldOutputAsync($"Unknown order: {input}", ct);
                return;
            }
            var daysLate = _orders.Today().DayNumber - order.PromisedBy.DayNumber;
            var isLate = order.Status == OrderStatus.Shipped && daysLate > 0;
            var statusLine = $"{order.Status} for {order.CustomerName} ({(isLate ? $"{daysLate} day(s) late" : "on track")})";
            ConsoleUi.Trace($"node lookup: {order.OrderId} → {statusLine}");
            await context.SendMessageAsync(new OrderLookupResult(order.OrderId, statusLine, isLate), cancellationToken: ct);
        }
    }

    [YieldsOutput(typeof(string))]
    private sealed class EscalationExecutor : Executor<OrderLookupResult>
    {
        public EscalationExecutor() : base(id: "escalate", options: null, declareCrossRunShareable: true) { }
        public override async ValueTask HandleAsync(OrderLookupResult r, IWorkflowContext context, CancellationToken ct = default)
        {
            ConsoleUi.Trace($"node escalate: routing {r.OrderId} to customer service");
            await context.YieldOutputAsync(
                $"ESCALATION: {r.OrderId} — {r.StatusLine}. Notify customer service and offer 15% credit.", ct);
        }
    }

    [YieldsOutput(typeof(string))]
    private sealed class ConfirmationExecutor : Executor<OrderLookupResult>
    {
        public ConfirmationExecutor() : base(id: "confirm", options: null, declareCrossRunShareable: true) { }
        public override async ValueTask HandleAsync(OrderLookupResult r, IWorkflowContext context, CancellationToken ct = default)
        {
            ConsoleUi.Trace($"node confirm: {r.OrderId} is on track");
            await context.YieldOutputAsync($"OK: {r.OrderId} — {r.StatusLine}. No action required.", ct);
        }
    }
}
