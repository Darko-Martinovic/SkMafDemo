# SkMafDemo — Semantic Kernel vs. Microsoft Agent Framework

A single .NET solution that demonstrates **Semantic Kernel (SK)** and the **Microsoft Agent Framework (MAF)** side by side. The goal is to make the comparison concrete: every menu item runs in isolation, prints a labelled header explaining what it shows, and prints tool-call traces as `[trace]` lines so the framework plumbing is visible.

Business logic — the tool methods the model can call — lives **once** in `SkMafDemo.Core`. The SK and MAF projects are thin adapters over those Core methods. That is the central proof of the demo: the orchestration framework is a thin layer over plain, testable C#.

## Prerequisites

- **.NET 8 SDK** (or .NET 10 SDK — `dotnet` will fetch the net8.0 reference packs from NuGet automatically; the project was built and tested on `dotnet 10.0.102`).
- *(Optional)* **Ollama** running on `http://localhost:11434` for offline use. Pull a small model first, e.g. `ollama pull llama3.2`.
- *(Optional)* an **OpenAI** or **Azure OpenAI** key for online use.

The app **also runs with no provider configured** — every menu item still loads; demos that need a model print a short "would do" explanation instead of crashing. Demos 10 and 11 are pure-workflow and work offline regardless.

## Configure a provider (user-secrets)

Pick one. Secrets stay on your machine — they are never committed.

**OpenAI:**

```powershell
dotnet user-secrets --project src/SkMafDemo.Console set Ai:Provider "OpenAI"
dotnet user-secrets --project src/SkMafDemo.Console set Ai:ApiKey  "sk-..."
dotnet user-secrets --project src/SkMafDemo.Console set Ai:Model   "gpt-4o-mini"
```

**Azure OpenAI:**

```powershell
dotnet user-secrets --project src/SkMafDemo.Console set Ai:Provider   "AzureOpenAI"
dotnet user-secrets --project src/SkMafDemo.Console set Ai:Endpoint   "https://<your-resource>.openai.azure.com"
dotnet user-secrets --project src/SkMafDemo.Console set Ai:ApiKey     "<key>"
dotnet user-secrets --project src/SkMafDemo.Console set Ai:Deployment "<your-deployment-name>"
```

**Ollama (offline):**

```powershell
dotnet user-secrets --project src/SkMafDemo.Console set Ai:Provider "Ollama"
dotnet user-secrets --project src/SkMafDemo.Console set Ai:Model    "llama3.2"
# Optional override:
# dotnet user-secrets --project src/SkMafDemo.Console set Ai:Endpoint "http://localhost:11434"
```

Configuration precedence is **env vars → user-secrets → `appsettings.json`** (env wins).

## Run

```powershell
dotnet build
dotnet test                              # runs the Core unit tests
dotnet run --project src/SkMafDemo.Console
```

Pick a number, see the demo, press Enter to return to the menu. Type `0` to exit.

## Menu items

| # | Section          | What it demonstrates                                                     | Key SK/MAF surface                                                                 |
|--:|------------------|--------------------------------------------------------------------------|------------------------------------------------------------------------------------|
| 1 | Semantic Kernel  | Plain chat completion, streamed                                          | `Kernel`, `IChatCompletionService.GetStreamingChatMessageContentsAsync`            |
| 2 | Semantic Kernel  | Native function calling against shared Core tools                        | `KernelPlugin`, `[KernelFunction]`, `FunctionChoiceBehavior.Auto`, `IFunctionInvocationFilter` |
| 3 | Semantic Kernel  | Prompt-as-function (templated, named arguments)                          | `Kernel.CreateFunctionFromPrompt`, `KernelArguments`                               |
| 4 | Semantic Kernel  | Single agent with the OrderTools plugin attached                         | `ChatCompletionAgent`                                                              |
| 5 | Semantic Kernel  | Multi-agent group chat (Researcher → Writer → Editor)                    | `AgentGroupChat`, `SequentialSelectionStrategy`, custom `TerminationStrategy`      |
| 6 | Semantic Kernel  | Minimal RAG with keyword-fallback retrieval                              | Hand-built retriever + grounded prompt; honest "keyword fallback" disclosure       |
| 7 | Agent Framework  | Smallest MAF program: one agent, plain run                               | `IChatClient.AsAIAgent(...)`, `AIAgent.RunAsync`, `AgentResponse.Text`             |
| 8 | Agent Framework  | Agent with tools — same Core methods as SK #2                            | `AIFunctionFactory.Create`, `AsAIAgent(tools: ...)`, `FunctionCallContent` tracing |
| 9 | Agent Framework  | Agents-as-tools: a Coordinator delegates to a Researcher                 | `AIFunctionFactory.Create(...)` over `Researcher.RunAsync`                         |
|10 | Agent Framework  | Explicit workflow graph with a conditional branch                        | `WorkflowBuilder`, `Executor<T>`, `[SendsMessage]`, `[YieldsOutput]`, `WithOutputFrom` |
|11 | Agent Framework  | Human-in-the-loop: workflow pauses for y/n, resumes                      | `RequestPort.Create<TReq,TResp>(...)`, `RequestInfoEvent`, `StreamingRun.SendResponseAsync` |
|12 | Side by side     | Same prompt + same Core tools through SK #2 and MAF #8                   | Reuses the Sk02 and Maf08 helper methods                                           |

## SK vs MAF — where each leads

**Semantic Kernel** is most comfortable when you want a *plug-in oriented kernel* with first-class **prompt functions** (`CreateFunctionFromPrompt`) and multi-agent orchestration via `AgentGroupChat` with explicit `Selection`/`Termination` strategies (item #5). The plugin model treats AI calls as named, parametrised "functions" — closer to how an enterprise codebase thinks about composition.

**Microsoft Agent Framework** leads on **explicit workflows** (item #10) and **human-in-the-loop** (item #11): the graph is a first-class artifact, executors declare their `SendsMessage`/`YieldsOutput` types, conditional edges route messages, and `RequestPort` turns "pause for a human" into a single primitive. Tool wiring is also lighter — `AIFunctionFactory.Create(method)` reads the `[Description]` attributes that already live on the Core methods, so the adapter does almost nothing.

Both frameworks consume `Microsoft.Extensions.AI.IChatClient`, so the same model client serves both adapters with no per-framework re-detection.

## Project layout

```
SkMafDemo.slnx
├── src/
│   ├── SkMafDemo.Core/            framework-agnostic domain + tools (NO SK/MAF refs)
│   │   ├── Domain/                  Order, OrderRepository (in-memory, ~5 seeded), KnowledgeBase
│   │   ├── Tools/                   GetOrderStatus, CalculateOrderTotal, GetShippingEstimate, GetWeather
│   │   └── Abstractions/            IChatModelConfig, Prompts
│   ├── SkMafDemo.SemanticKernel/  SK adapters: KernelPlugins wrapping Core.Tools, Kernel factory
│   ├── SkMafDemo.AgentFramework/  MAF adapters: AITool wrappers over Core.Tools, AIAgent factory
│   └── SkMafDemo.Console/         menu host, provider detection, all 12 demos
└── tests/
    └── SkMafDemo.Core.Tests/      xUnit + FluentAssertions over every Core tool
```

A grep for `Microsoft.SemanticKernel` and `Microsoft.Agents.AI` in `src/SkMafDemo.Core` returns zero results — that invariant is the demo's point.

## Notes on package versions (drift from the brief)

The brief pinned approximate versions; the project was built against the latest stable resolved by NuGet at build time. Drift recorded for honesty:

| Package                                | Brief said    | Installed       | Notes                                                                |
|----------------------------------------|---------------|------------------|----------------------------------------------------------------------|
| `Microsoft.Extensions.AI.Abstractions` | latest stable | `10.6.0`         | —                                                                    |
| `Microsoft.Extensions.AI.OpenAI`       | (not pinned)  | `10.6.0`         | Used for the `OpenAIClient.GetChatClient(...).AsIChatClient()` bridge |
| `Microsoft.SemanticKernel`             | ~1.7x         | `1.76.0`         | —                                                                    |
| `Microsoft.SemanticKernel.Agents.Core` | matching      | `1.76.0`         | `ChatCompletionAgent` / `AgentGroupChat` are marked `[Experimental]` — `#pragma warning disable SKEXP0001, SKEXP0110` at the call sites. |
| `Microsoft.Agents.AI`                  | 1.x latest    | `1.6.2`          | Verified surface uses `IChatClient.AsAIAgent(...)` (the brief mentioned `CreateAIAgent`; the shipped name is `AsAIAgent`). |
| `Microsoft.Agents.AI.OpenAI`           | matching      | `1.6.2`          | —                                                                    |
| `Microsoft.Agents.AI.Workflows`        | matching      | `1.6.2`          | Executors must declare `[SendsMessage]`/`[YieldsOutput]` types; `WithOutputFrom` is required for `YieldOutputAsync` to surface as a `WorkflowOutputEvent`. |
| `Azure.AI.OpenAI`                      | (transitive)  | `2.9.0-beta.1`   | Required by `Microsoft.SemanticKernel.Connectors.AzureOpenAI 1.76.0`; NU1605 forces matching version. |
| `OpenAI`                               | (not pinned)  | `2.10.0`         | Also targets Ollama via its OpenAI-compatible `/v1` endpoint.        |
| `OllamaSharp`                          | (not pinned)  | `5.4.25`         | `OllamaApiClient` implements `IChatClient` directly.                  |

## Acceptance-criteria checklist

- [x] `dotnet build` succeeds with no errors and no warnings.
- [x] `SkMafDemo.Core` references neither SK nor MAF (`.csproj` only references `Microsoft.Extensions.AI.Abstractions`).
- [x] The four Core tools exist once in Core and are **adapted**, not reimplemented, by both projects.
- [x] All 12 menu items run without crashing offline and print a meaningful explanation when no cloud model is set.
- [x] Items 2, 8, and 12 visibly invoke a shared Core tool and print the tool-call trace when a provider is configured.
- [x] Item 5 runs a real SK `AgentGroupChat`; item 10 runs a real MAF graph workflow with a conditional branch; item 11 pauses and resumes.
- [x] `dotnet test` passes (16 tests); every Core tool has at least one test.
- [x] README documents provider setup, run command, and the menu-to-concept mapping.
- [x] No secrets committed (`user-secrets` only).

## Continuous integration

Claude Code GitHub Actions are configured for this repo: opened/updated PRs receive an automated review, and `@claude` mentions in issues or PR comments are answered by Claude.
