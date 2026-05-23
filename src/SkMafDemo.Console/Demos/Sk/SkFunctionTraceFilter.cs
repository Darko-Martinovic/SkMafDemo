using Microsoft.SemanticKernel;
using SkMafDemo.Console.Ui;

namespace SkMafDemo.Console.Demos.Sk;

// SK function filter that prints a [trace] line for every tool the model decides
// to call. Hooks SK's IFunctionInvocationFilter — the same extension point any
// production SK code would use for telemetry/audit.
internal sealed class SkFunctionTraceFilter : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, Task> next)
    {
        var argsRendered = string.Join(", ",
            context.Arguments.Select(a => $"{a.Key}={Render(a.Value)}"));

        await next(context);

        var result = context.Result?.GetValue<object>()?.ToString() ?? "(no value)";
        ConsoleUi.Trace($"{context.Function.PluginName}.{context.Function.Name}({argsRendered}) -> {Truncate(result, 200)}");
    }

    private static string Render(object? value) => value switch
    {
        null => "null",
        string s => $"\"{s}\"",
        _ => value.ToString() ?? "null"
    };

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}
