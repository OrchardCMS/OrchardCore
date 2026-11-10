using System.Diagnostics;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;

namespace OrchardCore.Tests.Scripting;

public class JavaScriptEngineCancellationTests
{
    // Long enough that a test failing to cancel is unmistakable, short enough that such a failure still
    // finishes rather than hanging the run.
    private const string LongRunningScript = "var n = 0; for (var i = 0; i < 200000000; i++) { n += i; } return n;";

    [Fact]
    public async Task EvaluateAsync_WithAnAlreadyCancelledToken_DoesNotRunTheScript()
    {
        var (engine, scope) = CreateScope();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var stopwatch = Stopwatch.StartNew();

        // Before the constraint was armed here the token was only observed while awaiting promise
        // settlement, so a script that never yields ran to completion with the token already cancelled.
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => engine.EvaluateAsync(scope, LongRunningScript, cts.Token));

        stopwatch.Stop();

        // The script itself takes seconds; failing before it starts is the observable difference.
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2), $"took {stopwatch.Elapsed}");
    }

    [Fact]
    public async Task EvaluateAsync_WhenTheTokenIsCancelledWhileTheScriptRuns_StopsTheScript()
    {
        var (engine, scope) = CreateScope();

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => engine.EvaluateAsync(scope, LongRunningScript, cts.Token));
    }

    [Fact]
    public async Task EvaluateAsync_CarriesTheTokenThatWasCancelled()
    {
        var (engine, scope) = CreateScope();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // A host filtering with 'when (e is not OperationCanceledException)' needs the cancellation it
        // asked for to be distinguishable from a script failure, and needs the token to identify it. A
        // script this small never reaches an amortized constraint check, so this also pins that a token
        // cancelled before the call is observed before the script starts rather than not at all.
        var exception = await Assert.ThrowsAsync<OperationCanceledException>(
            () => engine.EvaluateAsync(scope, "return 1 + 1;", cts.Token));

        Assert.Equal(cts.Token, exception.CancellationToken);
    }

    [Fact]
    public async Task EvaluateAsync_WithoutACancellableToken_IsUnaffected()
    {
        var (engine, scope) = CreateScope();

        // CancellationToken.None is what the parameter defaults to, and CanBeCanceled is false for it, so
        // this is the path every caller in this repository takes today.
        Assert.Equal(2, Convert.ToInt32(await engine.EvaluateAsync(scope, "return 1 + 1;", CancellationToken.None)));
    }

    [Fact]
    public async Task EvaluateAsync_AfterACancelledEvaluation_TheSameScopeStillWorks()
    {
        var (engine, scope) = CreateScope();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => engine.EvaluateAsync(scope, "return 1 + 1;", cts.Token));

        // The constraint is disarmed in a finally, so the finished evaluation's token is not carried into
        // the next one. A scope is held for a whole request by JavascriptConditionEvaluator, so an engine
        // really does outlive the evaluation that cancelled.
        Assert.Equal(2, Convert.ToInt32(await engine.EvaluateAsync(scope, "return 1 + 1;", CancellationToken.None)));
        Assert.Equal(2, Convert.ToInt32(engine.Evaluate(scope, "return 1 + 1;")));
    }

    [Fact]
    public void Evaluate_TheSynchronousPath_IsUnaffected()
    {
        // Nothing arms the constraint on this path, and a disarmed constraint bounds nothing.
        var (engine, scope) = CreateScope();

        Assert.Equal(2, Convert.ToInt32(engine.Evaluate(scope, "return 1 + 1;")));
    }

    private static (IScriptingEngine Engine, IScriptingScope Scope) CreateScope()
    {
        var serviceProvider = new ServiceCollection()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine()
            .BuildServiceProvider();

        var engine = serviceProvider.GetServices<IScriptingEngine>().First(engine => engine.Prefix == "js");

        return (engine, engine.CreateScope([], serviceProvider, null, null));
    }
}
