using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using OpenAI;
using SkMafDemo.Core.Abstractions;

namespace SkMafDemo.Console.Hosting;

// Bundles the configured chat-model info with the native SDK clients each framework
// wants. MAF prefers an IChatClient (Microsoft.Extensions.AI); SK's connectors prefer
// the OpenAI / Azure SDK clients directly. We carry both so each adapter can use its
// idiomatic path without re-detecting the provider.
internal sealed class ChatClientBundle
{
    public IChatModelConfig Config { get; }
    public IChatClient? ChatClient { get; }
    public OpenAIClient? OpenAIClient { get; }
    public AzureOpenAIClient? AzureOpenAIClient { get; }

    public bool HasChatClient => ChatClient is not null;

    public ChatClientBundle(
        IChatModelConfig config,
        IChatClient? chatClient,
        OpenAIClient? openAIClient,
        AzureOpenAIClient? azureOpenAIClient)
    {
        Config = config;
        ChatClient = chatClient;
        OpenAIClient = openAIClient;
        AzureOpenAIClient = azureOpenAIClient;
    }

    public static ChatClientBundle Unconfigured(IChatModelConfig config) =>
        new(config, null, null, null);
}
