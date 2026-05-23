using SkMafDemo.Core.Abstractions;

namespace SkMafDemo.Console.Hosting;

internal sealed record ChatModelConfig(
    ChatProvider Provider,
    string Model,
    string Endpoint,
    bool IsConfigured) : IChatModelConfig
{
    public static ChatModelConfig Unconfigured() =>
        new(ChatProvider.None, "(none)", "(none)", false);
}
