using OrchardCore.Rules.Models;
using OrchardCore.Rules.Services;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;

namespace OrchardCore.Tests.Modules.OrchardCore.Rules;

/// <summary>
/// The rules evaluator is the one caller that keeps a scripting scope alive for a whole request instead of
/// for a single expression, so it is also the one that has to hand it back when the request ends.
/// </summary>
public class JavascriptConditionEvaluatorTests
{
    [Fact]
    public async Task EvaluateAsync_ForSeveralConditions_KeepsTheEngineButNotTheGlobalsOfEarlierConditions()
    {
        var (services, scriptingEngine) = CreateScriptingServices();

        var warmUp = (JavaScriptScope)scriptingEngine.CreateScope([], services, null, null);
        var engine = warmUp.Engine;
        warmUp.Dispose();

        using var evaluator = new JavascriptConditionEvaluator(services.GetRequiredService<IScriptingManager>(), services);

        // The conditions of different layers are written independently, so the second must not see what the
        // first declared, even though both run on the scope the evaluator holds for the whole request.
        Assert.True(await evaluator.EvaluateAsync(new JavascriptCondition { Script = "globalThis.n = 1; var declared = 1; const constant = 1; return true;" }));
        Assert.True(await evaluator.EvaluateAsync(new JavascriptCondition { Script = "return typeof n === 'undefined' && typeof declared === 'undefined' && typeof constant === 'undefined';" }));

        // The evaluator still holds the tenant's one pooled engine: it is not handed back between conditions.
        using var meanwhile = (JavaScriptScope)scriptingEngine.CreateScope([], services, null, null);

        Assert.NotSame(engine, meanwhile.Engine);
    }

    [Fact]
    public async Task Dispose_AtTheEndOfTheRequest_ReleasesTheEngineForReuse()
    {
        var (services, scriptingEngine) = CreateScriptingServices();

        // With a single pooled engine, the identity of the one the pool hands out is enough to tell whether
        // the evaluator gave its engine back.
        var warmUp = (JavaScriptScope)scriptingEngine.CreateScope([], services, null, null);
        var engine = warmUp.Engine;
        warmUp.Dispose();

        var evaluator = new JavascriptConditionEvaluator(services.GetRequiredService<IScriptingManager>(), services);

        Assert.True(await evaluator.EvaluateAsync(new JavascriptCondition { Script = "return true;" }));

        evaluator.Dispose();

        using var afterRequest = (JavaScriptScope)scriptingEngine.CreateScope([], services, null, null);

        Assert.Same(engine, afterRequest.Engine);

        // And what the request's conditions left behind did not come back with it.
        Assert.Equal("undefined", scriptingEngine.Evaluate(afterRequest, "return typeof n;"));
    }

    [Fact]
    public async Task EvaluateAsync_AfterDisposal_Throws()
    {
        var (services, _) = CreateScriptingServices();

        var evaluator = new JavascriptConditionEvaluator(services.GetRequiredService<IScriptingManager>(), services);
        evaluator.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            async () => await evaluator.EvaluateAsync(new JavascriptCondition { Script = "return true;" }));
    }

    [Fact]
    public async Task ARequestAbortedMidCondition_StopsTheScriptAndLeavesTheEngineForTheNextRequest()
    {
        // The conditions of a request run on the request-aborted token, on the pooled engine the evaluator
        // holds for the whole request. A client going away in the middle of a condition must stop it, and
        // the engine it ran on must still be handed back clean, with that token no longer armed on it.
        var accessor = new HttpContextAccessor();
        var gaveUpAfter = TimeSpan.FromSeconds(10);

        using var firstRequestAborted = new CancellationTokenSource();

        var services = new ServiceCollection()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine()
            .Configure<JavaScriptEngineOptions>(options => options.EnginePoolSize = 1)
            .AddSingleton<IHttpContextAccessor>(accessor)
            .AddSingleton<IGlobalMethodProvider>(new RequestMethodProvider(
                abort: firstRequestAborted.Cancel,
                gaveUp: CreateGaveUp(gaveUpAfter)))
            .BuildServiceProvider();

        var scriptingManager = services.GetRequiredService<IScriptingManager>();
        var scriptingEngine = scriptingManager.GetScriptingEngine("js");

        var warmUp = (JavaScriptScope)scriptingEngine.CreateScope([], services, null, null);
        var engine = warmUp.Engine;
        warmUp.Dispose();

        accessor.HttpContext = new DefaultHttpContext { RequestAborted = firstRequestAborted.Token };

        var firstRequest = new JavascriptConditionEvaluator(scriptingManager, services);

        try
        {
            // The client goes away once the condition is running, and the loop would otherwise only end when
            // it gives up, returning true: a condition that was not stopped fails the assertion rather than
            // hanging the test.
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await firstRequest.EvaluateAsync(new JavascriptCondition { Script = "globalThis.dirty = 1; abort(); while (!gaveUp()) { } return true;" }));
        }
        finally
        {
            firstRequest.Dispose();
        }

        using (var between = (JavaScriptScope)scriptingEngine.CreateScope([], services, null, null))
        {
            Assert.Same(engine, between.Engine);
        }

        using var secondRequestAborted = new CancellationTokenSource();
        accessor.HttpContext = new DefaultHttpContext { RequestAborted = secondRequestAborted.Token };

        using var secondRequest = new JavascriptConditionEvaluator(scriptingManager, services);

        Assert.True(await secondRequest.EvaluateAsync(new JavascriptCondition { Script = "return typeof dirty === 'undefined';" }));

        // A loop long enough to reach the deadline constraint's amortized check several times over would be
        // stopped if the first request's cancelled token were still armed on the engine.
        Assert.True(await secondRequest.EvaluateAsync(new JavascriptCondition { Script = "var n = 0; for (var i = 0; i < 10000; i++) { n++; } return n === 10000;" }));
    }

    private static Func<bool> CreateGaveUp(TimeSpan after)
    {
        // Started by the first call, so the time is counted from when the condition began looping.
        var stopwatch = new System.Diagnostics.Stopwatch();

        return () =>
        {
            stopwatch.Start();

            return stopwatch.Elapsed >= after;
        };
    }

    private static (IServiceProvider Services, IScriptingEngine ScriptingEngine) CreateScriptingServices()
    {
        var services = new ServiceCollection()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine()
            .Configure<JavaScriptEngineOptions>(options => options.EnginePoolSize = 1)
            .BuildServiceProvider();

        return (services, services.GetRequiredService<IScriptingManager>().GetScriptingEngine("js"));
    }

    private sealed class RequestMethodProvider : IGlobalMethodProvider
    {
        private readonly Action _abort;
        private readonly Func<bool> _gaveUp;

        public RequestMethodProvider(Action abort, Func<bool> gaveUp)
        {
            _abort = abort;
            _gaveUp = gaveUp;
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            yield return new GlobalMethod
            {
                Name = "abort",
                Method = serviceProvider => _abort,
            };

            yield return new GlobalMethod
            {
                Name = "gaveUp",
                Method = serviceProvider => _gaveUp,
            };
        }
    }
}
