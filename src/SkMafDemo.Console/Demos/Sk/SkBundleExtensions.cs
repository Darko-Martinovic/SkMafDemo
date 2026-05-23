using Microsoft.SemanticKernel;
using SkMafDemo.Console.Hosting;
using SkMafDemo.Core.Abstractions;
using SkMafDemo.SemanticKernel;

namespace SkMafDemo.Console.Demos.Sk;

internal static class SkBundleExtensions
{
    // Builds a fresh Kernel for the active provider. SK Kernels are cheap, so a new
    // one per demo run is fine — and isolates plugin state between demos.
    public static Kernel CreateKernel(this ChatClientBundle bundle)
    {
        return bundle.Config.Provider switch
        {
            ChatProvider.AzureOpenAI when bundle.AzureOpenAIClient is not null =>
                SkKernelFactory.CreateAzureOpenAIKernel(bundle.AzureOpenAIClient, bundle.Config.Model),
            ChatProvider.OpenAI when bundle.OpenAIClient is not null =>
                SkKernelFactory.CreateOpenAIKernel(bundle.OpenAIClient, bundle.Config.Model),
            ChatProvider.Ollama when bundle.OpenAIClient is not null =>
                // SK talks to Ollama through Ollama's OpenAI-compatible /v1 endpoint.
                SkKernelFactory.CreateOpenAIKernel(bundle.OpenAIClient, bundle.Config.Model),
            _ => throw new InvalidOperationException(
                $"SK demos need an OpenAI- or Azure-compatible client; provider {bundle.Config.Provider} has none configured.")
        };
    }
}
