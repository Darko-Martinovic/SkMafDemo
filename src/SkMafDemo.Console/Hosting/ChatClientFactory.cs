using System.ClientModel;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OllamaSharp;
using OpenAI;
using SkMafDemo.Core.Abstractions;

namespace SkMafDemo.Console.Hosting;

internal static class ChatClientFactory
{
    // Reads Ai:* config in env → user-secrets → appsettings order (precedence is set
    // up by the host builder; this method just reads). Builds whatever client(s) the
    // selected provider needs and returns them in a bundle, OR an Unconfigured bundle
    // when no provider is set — the menu still works in that case, each demo just
    // prints an explanation instead of calling a model.
    public static ChatClientBundle Build(IConfiguration config)
    {
        var providerName = config["Ai:Provider"];
        if (!Enum.TryParse<ChatProvider>(providerName, ignoreCase: true, out var provider))
        {
            provider = ChatProvider.None;
        }

        return provider switch
        {
            ChatProvider.OpenAI => BuildOpenAI(config),
            ChatProvider.AzureOpenAI => BuildAzureOpenAI(config),
            ChatProvider.Ollama => BuildOllama(config),
            _ => ChatClientBundle.Unconfigured(ChatModelConfig.Unconfigured())
        };
    }

    private static ChatClientBundle BuildOpenAI(IConfiguration config)
    {
        var apiKey = config["Ai:ApiKey"];
        var model = config["Ai:Model"] ?? "gpt-4o-mini";
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return ChatClientBundle.Unconfigured(
                new ChatModelConfig(ChatProvider.OpenAI, model, "(api.openai.com)", false));
        }
        var openAIClient = new OpenAIClient(new ApiKeyCredential(apiKey));
        var chatClient = openAIClient.GetChatClient(model).AsIChatClient();
        var meta = new ChatModelConfig(ChatProvider.OpenAI, model, "api.openai.com", true);
        return new ChatClientBundle(meta, chatClient, openAIClient, null);
    }

    private static ChatClientBundle BuildAzureOpenAI(IConfiguration config)
    {
        var endpoint = config["Ai:Endpoint"];
        var apiKey = config["Ai:ApiKey"];
        var deployment = config["Ai:Deployment"] ?? config["Ai:Model"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(deployment) || string.IsNullOrWhiteSpace(apiKey))
        {
            return ChatClientBundle.Unconfigured(
                new ChatModelConfig(ChatProvider.AzureOpenAI, deployment, endpoint ?? "(missing)", false));
        }

        var azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        var chatClient = azureClient.GetChatClient(deployment).AsIChatClient();
        var meta = new ChatModelConfig(ChatProvider.AzureOpenAI, deployment, endpoint, true);
        return new ChatClientBundle(meta, chatClient, null, azureClient);
    }

    private static ChatClientBundle BuildOllama(IConfiguration config)
    {
        // Default to the standard local endpoint so the demo "just works" when Ollama
        // is installed. Model defaults to llama3.2 — the user can override via config.
        var endpoint = string.IsNullOrWhiteSpace(config["Ai:Endpoint"])
            ? "http://localhost:11434"
            : config["Ai:Endpoint"]!;
        var model = string.IsNullOrWhiteSpace(config["Ai:Model"]) ? "llama3.2" : config["Ai:Model"]!;

        // OllamaApiClient implements IChatClient directly, so MAF can consume it as-is.
        // For SK we also build an OpenAIClient pointed at Ollama's OpenAI-compatible
        // /v1 endpoint, so SK's native AddOpenAIChatCompletion connector works too.
        var ollama = new OllamaApiClient(new Uri(endpoint)) { SelectedModel = model };

        var openAIClient = new OpenAIClient(
            new ApiKeyCredential("ollama"),
            new OpenAIClientOptions { Endpoint = new Uri(endpoint.TrimEnd('/') + "/v1") });

        var meta = new ChatModelConfig(ChatProvider.Ollama, model, endpoint, true);
        return new ChatClientBundle(meta, ollama, openAIClient, null);
    }
}
