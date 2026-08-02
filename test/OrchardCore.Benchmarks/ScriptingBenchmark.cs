using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OrchardCore.Contents.Scripting;
using OrchardCore.Entities;
using OrchardCore.Entities.Scripting;
using OrchardCore.Layers.Services;
using OrchardCore.Modules;
using OrchardCore.Queries;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;
using OrchardCore.Scripting.Providers;
using OrchardCore.Workflows.Http.Scripting;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Scripting;

namespace OrchardCore.Benchmarks;

/// <summary>
/// Measures the JavaScript scripting paths the way Orchard Core actually drives them, which is one engine
/// per evaluated expression: <see cref="DefaultScriptingManager"/> builds a scope — and with it an
/// <c>Engine</c> — for every directive it evaluates, and the workflow and recipe call sites go through it
/// once per expression.
/// </summary>
/// <remarks>
/// <para>
/// Engine construction is therefore deliberately inside the measured method rather than in
/// <c>[GlobalSetup]</c>: it is the cost under study, not setup to be hoisted out of the way. Rows that do
/// reuse a scope (<c>LayerRule_*</c>) mirror <c>JavascriptConditionEvaluator</c>, which caches one scope for
/// the lifetime of a request and evaluates every JavaScript layer rule on it.
/// </para>
/// <para>
/// The provider set is the public subset of what a CMS tenant registers: <c>DataProtectionMethods</c> is
/// internal to <c>OrchardCore.Scripting</c> and cannot be constructed from here, so the global count is a
/// couple short of a real site's. Everything else — the number of providers, the mix of synchronous and
/// asynchronous methods, the shape of the scripts — is taken from the real registrations and from the
/// <c>[js:…]</c> expressions in this repository's own recipes.
/// </para>
/// <para>
/// <see cref="DefaultScriptingManager"/> hands <c>ShellScope.Services</c> to the scope it builds, and there
/// is no shell scope here, so the global a row actually calls has to be one whose delegate does not resolve a
/// service. That rules out <c>uuid</c>, which resolves <c>IIdGenerator</c> when called; the "reads one
/// registered global" row therefore calls <see cref="TokenMethodProvider"/>, a provider declared here whose
/// delegate needs nothing. The real providers stay registered either way, so the per-engine cost of declaring
/// their globals is still in every row — they are simply never read. The rows that reuse a scope pass a real
/// service provider and can call whatever they like.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class ScriptingBenchmark
{
    private IServiceProvider _serviceProvider;
    private IScriptingManager _scriptingManager;
    private IScriptingEngine _jsEngine;
    private GlobalMethod[] _registeredMethods;
    private WorkflowMethodsProvider _workflowMethods;
    private IGlobalMethodProvider[] _workflowProviders;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder => builder.ClearProviders());
        services.AddMemoryCache();
        services.AddScripting();
        services.AddJavaScriptEngine();

        // The public providers a CMS tenant registers. See the remarks for the one that is missing.
        services.AddSingleton<IGlobalMethodProvider, IdGeneratorMethod>();
        services.AddSingleton<IGlobalMethodProvider, ContentMethodsProvider>();
        services.AddSingleton<IGlobalMethodProvider, UrlMethodsProvider>();
        services.AddSingleton<IGlobalMethodProvider, DefaultLayersMethodProvider>();
        services.AddSingleton<IGlobalMethodProvider, QueryGlobalMethodProvider>();
        services.AddSingleton<IGlobalMethodProvider, LogProvider>();
        services.AddSingleton<IGlobalMethodProvider, ProtectDataProvider>();
        services.AddSingleton<IGlobalMethodProvider, HttpMethodsProvider>();

        // The one global the manager-driven rows call. See the remarks for why it cannot be uuid.
        services.AddSingleton<IGlobalMethodProvider, TokenMethodProvider>();

        services.AddSingleton<IIdGenerator, DefaultIdGenerator>();

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = new PathString("/");
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        services.AddSingleton(httpContextAccessor.Object);

        _serviceProvider = services.BuildServiceProvider();
        _scriptingManager = _serviceProvider.GetRequiredService<IScriptingManager>();
        _jsEngine = _scriptingManager.GetScriptingEngine("js");
        _registeredMethods = _scriptingManager.GlobalMethodProviders.SelectMany(x => x.GetMethods()).ToArray();

        var workflowContext = new WorkflowExecutionContext(
            new WorkflowType { Id = 1, WorkflowTypeId = "wt", Activities = [], Transitions = [] },
            new Workflow { WorkflowId = "w1", State = new System.Text.Json.Nodes.JsonObject() },
            new Dictionary<string, object> { ["Message"] = "hello" },
            new Dictionary<string, object>(),
            new Dictionary<string, object>(),
            [],
            null,
            []);

        _workflowMethods = new WorkflowMethodsProvider(workflowContext);
        _workflowProviders = [_workflowMethods];

        // Warm every path once so no row pays for first-call JIT or for the prepared-script cache miss.
        Recipe_OneGlobal();
        Recipe_Constant();
        Workflow_Input().GetAwaiter().GetResult();
        Workflow_Composite().GetAwaiter().GetResult();
        LayerRule_PerRequest_ThreeRules().GetAwaiter().GetResult();
    }

    /// <summary>
    /// The commonest recipe directive shape: one engine, one call into one registered global.
    /// </summary>
    [Benchmark(Baseline = true)]
    public object Recipe_OneGlobal()
        => _scriptingManager.Evaluate("js:token()", null, null, null);

    /// <summary>
    /// The floor: an engine that evaluates an expression touching no global at all. The difference from
    /// <see cref="Recipe_OneGlobal"/> is what materializing one global costs; the row itself is what the
    /// engine costs — construction plus declaring every registered global on it.
    /// </summary>
    [Benchmark]
    public object Recipe_Constant()
        => _scriptingManager.Evaluate("js:'literal'", null, null, null);

    /// <summary>
    /// A workflow expression. The nine <see cref="WorkflowMethodsProvider"/> globals are passed to the scope
    /// rather than registered in DI, so they are on the eager path and every one of them is built and
    /// wrapped for an engine that only reads <c>input</c>.
    /// </summary>
    [Benchmark]
    public async Task<object> Workflow_Input()
        => await _scriptingManager.EvaluateAsync("js:input('Message')", null, null, _workflowProviders);

    /// <summary>
    /// A workflow expression that reads and writes several of the scoped globals, so the eager cost is at
    /// least partly paid for.
    /// </summary>
    [Benchmark]
    public async Task<object> Workflow_Composite()
        => await _scriptingManager.EvaluateAsync(
            "js:(function(){ setProperty('seen', input('Message')); return workflowId() + ':' + property('seen'); })()",
            null,
            null,
            _workflowProviders);

    /// <summary>
    /// One request's worth of JavaScript layer rules: a single scope, reused across three distinct rules,
    /// the way <c>JavascriptConditionEvaluator</c> does it.
    /// </summary>
    /// <remarks>
    /// The scope is released at the end, because that is what the evaluator's own lifetime does — it is a
    /// scoped service, so the container disposes it when the request ends. Holding the scope instead would
    /// measure a leak: an implementation that hands out reusable engines would never get this one back, and
    /// the row would report the cost of that mistake rather than the cost of the request.
    /// <see langword="as"/> rather than a cast, since a scope is not required to be disposable.
    /// </remarks>
    [Benchmark]
    public async Task<bool> LayerRule_PerRequest_ThreeRules()
    {
        var scope = _jsEngine.CreateScope(_registeredMethods, _serviceProvider, null, null);

        try
        {
            return await EvaluateThreeRulesAsync(scope);
        }
        finally
        {
            (scope as IDisposable)?.Dispose();
        }
    }

    private async Task<bool> EvaluateThreeRulesAsync(IScriptingScope scope)
    {
        var a = Convert.ToBoolean(await _jsEngine.EvaluateAsync(scope, "isHomepage()"));
        var b = Convert.ToBoolean(await _jsEngine.EvaluateAsync(scope, "isAuthenticated()"));
        var c = Convert.ToBoolean(await _jsEngine.EvaluateAsync(scope, "url('/about')"));

        return a || b || c;
    }

    /// <summary>
    /// A registered global whose delegate resolves nothing, so it can be called from the rows that go through
    /// <see cref="DefaultScriptingManager"/> and therefore have no service provider. See the class remarks.
    /// </summary>
    private sealed class TokenMethodProvider : IGlobalMethodProvider
    {
        private static readonly GlobalMethod _token = new()
        {
            Name = "token",
            Method = _ => (Func<string>)(() => "token"),
        };

        public IEnumerable<GlobalMethod> GetMethods() => [_token];
    }
}
