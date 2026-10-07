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
}
