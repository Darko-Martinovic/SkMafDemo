using Microsoft.Agents.AI.Workflows;
using SkMafDemo.Console.Menu;
using SkMafDemo.Console.Ui;
using SkMafDemo.Core.Domain;

namespace SkMafDemo.Console.Demos.Maf;

// What it shows: MAF's human-in-the-loop pattern via RequestPort. The workflow
// routes a request to an external port → the run pauses with RunStatus.PendingRequests
// → we read a y/n from the console → resume with the response → the workflow finishes.
// No LLM needed for the workflow plumbing.
internal sealed class Maf11_HumanInTheLoopDemo : IDemo
{
    private readonly OrderRepository _orders;

    public int Number => 11;
    public string Section => Sections.AgentFramework;
    public string Title => "MAF: Human-in-the-loop workflow (pause for approval, resume)";

    public Maf11_HumanInTheLoopDemo(OrderRepository orders) => _orders = orders;

    public async Task RunAsync(CancellationToken ct)
    {
        ConsoleUi.Header($"{Number}) {Title}",
            "Decision → Approval port → Outcome. Pauses for y/n, then resumes.");

        var decision = new DecisionExecutor(_orders);
        var approval = RequestPort.Create<ApprovalRequest, bool>("approval");
        var outcome = new OutcomeExecutor();

        var workflow = new WorkflowBuilder(decision)
            .AddEdge(decision, approval)
            .AddEdge(approval, outcome)
            // Mark which executors' YieldOutputAsync calls bubble as WorkflowOutputEvent.
            .WithOutputFrom(decision, outcome)
            .Build();

        ConsoleUi.Trace("InProcessExecution.OpenStreamingAsync(workflow) → TrySendMessageAsync(\"ORD-10432\")");
        await using var run = await InProcessExecution.OpenStreamingAsync(workflow, cancellationToken: ct);
        await run.TrySendMessageAsync("ORD-10432");

        // Watch the stream. When a RequestInfoEvent arrives we pause for input.
        await foreach (var ev in run.WatchStreamAsync(ct))
        {
            switch (ev)
            {
                case RequestInfoEvent req when req.Request.TryGetDataAs<ApprovalRequest>(out var ask):
                    ConsoleUi.Trace("RequestInfoEvent received — workflow is paused.");
                    System.Console.WriteLine();
                    ConsoleUi.Warn($"APPROVAL NEEDED: {ask!.Question}");
                    System.Console.Write("Approve credit? (y/n) [n]: ");
                    var input = System.Console.ReadLine()?.Trim().ToLowerInvariant();
                    var approved = input is "y" or "yes";
                    ConsoleUi.Trace($"Resuming with response: {(approved ? "APPROVED" : "DENIED")}");
                    await run.SendResponseAsync(req.Request.CreateResponse(approved));
                    break;

                case WorkflowOutputEvent output:
                    System.Console.WriteLine();
                    ConsoleUi.Info($"Workflow output: {output.Data}");
                    break;
            }
        }
    }

    private sealed record ApprovalRequest(string OrderId, string Question);

    // MAF Workflows requires explicit declaration of outbound message types.
    [SendsMessage(typeof(ApprovalRequest))]
    [YieldsOutput(typeof(string))]
    private sealed class DecisionExecutor : Executor<string>
    {
        private readonly OrderRepository _orders;
        public DecisionExecutor(OrderRepository orders)
            : base(id: "decide", options: null, declareCrossRunShareable: true) => _orders = orders;

        public override async ValueTask HandleAsync(string orderId, IWorkflowContext context, CancellationToken ct = default)
        {
            var order = _orders.Find(orderId);
            if (order is null)
            {
                await context.YieldOutputAsync($"Unknown order: {orderId}", ct);
                return;
            }
            var daysLate = _orders.Today().DayNumber - order.PromisedBy.DayNumber;
            ConsoleUi.Trace($"node decide: {order.OrderId} is {daysLate} day(s) past promise → needs human approval");
            var request = new ApprovalRequest(
                order.OrderId,
                $"Order {order.OrderId} for {order.CustomerName} is {daysLate} day(s) late. " +
                "Approve a 15% goodwill credit?");
            await context.SendMessageAsync(request, cancellationToken: ct);
        }
    }

    [YieldsOutput(typeof(string))]
    private sealed class OutcomeExecutor : Executor<bool>
    {
        public OutcomeExecutor() : base(id: "outcome", options: null, declareCrossRunShareable: true) { }
        public override async ValueTask HandleAsync(bool approved, IWorkflowContext context, CancellationToken ct = default)
        {
            ConsoleUi.Trace($"node outcome: received {(approved ? "APPROVED" : "DENIED")}");
            var message = approved
                ? "Credit APPROVED. Refund queued."
                : "Credit DENIED. No further action.";
            await context.YieldOutputAsync(message, ct);
        }
    }
}
