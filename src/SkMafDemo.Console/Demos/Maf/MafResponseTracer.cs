using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using SkMafDemo.Console.Ui;

namespace SkMafDemo.Console.Demos.Maf;

// Walks an AgentRunResponse and prints a [trace] line for every tool call and
// tool result the agent produced. MAF surfaces the loop as a list of ChatMessages —
// tool calls show up as FunctionCallContent, tool results as FunctionResultContent.
internal static class MafResponseTracer
{
    public static void Trace(AgentResponse response)
    {
        foreach (var message in response.Messages)
        {
            foreach (var content in message.Contents)
            {
                switch (content)
                {
                    case FunctionCallContent call:
                        var args = call.Arguments is null
                            ? "(no args)"
                            : string.Join(", ", call.Arguments.Select(a => $"{a.Key}={Render(a.Value)}"));
                        ConsoleUi.Trace($"call: {call.Name}({args})");
                        break;
                    case FunctionResultContent result:
                        ConsoleUi.Trace($"result: {Truncate(result.Result?.ToString() ?? "(null)", 200)}");
                        break;
                }
            }
        }
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
