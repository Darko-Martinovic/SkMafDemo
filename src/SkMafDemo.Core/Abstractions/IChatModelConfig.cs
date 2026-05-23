namespace SkMafDemo.Core.Abstractions;

// Display + decision info about the currently-configured chat model.
// The actual IChatClient instance is registered separately in DI; this interface
// lets adapters & menu handlers reason about the provider without taking a hard
// dependency on the concrete client SDKs.
public interface IChatModelConfig
{
    ChatProvider Provider { get; }

    // Model id (OpenAI/Ollama) or deployment name (Azure OpenAI).
    string Model { get; }

    // Friendly endpoint string for display, e.g. "https://...openai.azure.com" or "http://localhost:11434".
    string Endpoint { get; }

    // False when no provider is configured. Menu handlers MUST check this and print
    // a "no model configured" explanation rather than crashing.
    bool IsConfigured { get; }
}
