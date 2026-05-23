using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using SkMafDemo.Core.Tools;

namespace SkMafDemo.AgentFramework;

// Adapter: builds MAF AIAgents and AIFunction tool collections from Core methods.
// MAF natively consumes IChatClient and reads [Description] attributes via
// AIFunctionFactory — so the Core tool methods can be wrapped with zero ceremony.
public static class MafAgentFactory
{
    public static AIAgent CreateAgent(
        IChatClient chatClient,
        string instructions,
        string name,
        IList<AITool>? tools = null) =>
        // Microsoft.Extensions.AI.ChatClientExtensions.AsAIAgent — the verified
        // extension on IChatClient in Microsoft.Agents.AI 1.6.2 (the brief mentioned
        // CreateAIAgent; the shipped name is AsAIAgent).
        chatClient.AsAIAgent(instructions: instructions, name: name, tools: tools);

    // Tool builders. AIFunctionFactory.Create reflects on the method and uses its
    // [Description] attributes for tool/parameter schema — the Core descriptions
    // become the descriptions the MAF agent shows the model.
    public static IList<AITool> BuildOrderTools(OrderTools tools) =>
        new AITool[]
        {
            AIFunctionFactory.Create(tools.GetOrderStatus),
            AIFunctionFactory.Create(tools.CalculateOrderTotal)
        };

    public static IList<AITool> BuildShippingTools(ShippingTools tools) =>
        new AITool[] { AIFunctionFactory.Create(tools.GetShippingEstimate) };

    public static IList<AITool> BuildWeatherTools(WeatherTools tools) =>
        new AITool[] { AIFunctionFactory.Create(tools.GetWeather) };
}
