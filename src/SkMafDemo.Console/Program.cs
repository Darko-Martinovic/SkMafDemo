using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkMafDemo.Console.Demos.Maf;
using SkMafDemo.Console.Demos.SideBySide;
using SkMafDemo.Console.Demos.Sk;
using SkMafDemo.Console.Hosting;
using SkMafDemo.Console.Menu;
using SkMafDemo.Core.Abstractions;
using SkMafDemo.Core.Domain;
using SkMafDemo.Core.Tools;

var builder = Host.CreateApplicationBuilder(args);

// Config precedence per §5: env → user-secrets → appsettings.json.
// CreateApplicationBuilder already adds appsettings.json + env vars; we layer
// user-secrets on top in Development mode, then env vars LAST so they win.
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables();

// Core: domain + tool services. Singletons because the repository is in-memory
// and the tool classes are stateless (they share the same repo).
builder.Services.AddSingleton<OrderRepository>();
builder.Services.AddSingleton<OrderTools>();
builder.Services.AddSingleton<ShippingTools>();
builder.Services.AddSingleton<WeatherTools>();

// Build the chat client bundle once at startup. If no provider is configured, the
// bundle has IsConfigured=false and each demo will print an explanation instead
// of crashing — see ConsoleUi/MenuRunner error handling.
var bundle = ChatClientFactory.Build(builder.Configuration);
builder.Services.AddSingleton(bundle);
builder.Services.AddSingleton<IChatModelConfig>(bundle.Config);

// Real SK demos. Anything still unimplemented falls through to the stubs below.
builder.Services.AddSingleton<IDemo, Sk01_PlainChatDemo>();
builder.Services.AddSingleton<IDemo, Sk02_FunctionCallingDemo>();
builder.Services.AddSingleton<IDemo, Sk03_SemanticFunctionDemo>();
builder.Services.AddSingleton<IDemo, Sk04_SingleAgentDemo>();
builder.Services.AddSingleton<IDemo, Sk05_GroupChatDemo>();
builder.Services.AddSingleton<IDemo, Sk06_RagDemo>();
builder.Services.AddSingleton<IDemo, Maf07_PlainAgentDemo>();
builder.Services.AddSingleton<IDemo, Maf08_AgentWithToolsDemo>();
builder.Services.AddSingleton<IDemo, Maf09_AgentsAsToolsDemo>();
builder.Services.AddSingleton<IDemo, Maf10_WorkflowDemo>();
builder.Services.AddSingleton<IDemo, Maf11_HumanInTheLoopDemo>();
builder.Services.AddSingleton<IDemo, SideBySideDemo>();

builder.Services.AddHostedService<MenuRunner>();

await builder.Build().RunAsync();
