using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;
using OpenAI;
using SkMafDemo.Core.Tools;

namespace SkMafDemo.SemanticKernel;

// Adapter: builds a configured SK Kernel from the provider-native client. Kept in
// this project so the Core stays SK-free and the Console stays free of SK plumbing.
public static class SkKernelFactory
{
    public static Kernel CreateOpenAIKernel(OpenAIClient openAIClient, string modelId)
    {
        var builder = Kernel.CreateBuilder();
        // Works for both real OpenAI and any OpenAI-compatible endpoint (Ollama's /v1).
        builder.AddOpenAIChatCompletion(modelId, openAIClient);
        return builder.Build();
    }

    public static Kernel CreateAzureOpenAIKernel(AzureOpenAIClient azureClient, string deployment)
    {
        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(deployment, azureClient);
        return builder.Build();
    }

    // Builds a KernelPlugin from the Core tool methods WITHOUT requiring SK attributes
    // on the Core methods. SK's KernelFunctionFactory reflects on the supplied method's
    // MethodInfo and reads its [Description] attributes — so the descriptions live in
    // Core, written once, and SK picks them up via the adapter.
    public static KernelPlugin BuildOrderPlugin(OrderTools tools) =>
        KernelPluginFactory.CreateFromFunctions("OrderTools", new[]
        {
            KernelFunctionFactory.CreateFromMethod(tools.GetOrderStatus, nameof(tools.GetOrderStatus)),
            KernelFunctionFactory.CreateFromMethod(tools.CalculateOrderTotal, nameof(tools.CalculateOrderTotal)),
        });

    public static KernelPlugin BuildShippingPlugin(ShippingTools tools) =>
        KernelPluginFactory.CreateFromFunctions("ShippingTools", new[]
        {
            KernelFunctionFactory.CreateFromMethod(tools.GetShippingEstimate, nameof(tools.GetShippingEstimate)),
        });

    public static KernelPlugin BuildWeatherPlugin(WeatherTools tools) =>
        KernelPluginFactory.CreateFromFunctions("WeatherTools", new[]
        {
            KernelFunctionFactory.CreateFromMethod(tools.GetWeather, nameof(tools.GetWeather)),
        });
}
