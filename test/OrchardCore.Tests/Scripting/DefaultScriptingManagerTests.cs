using OrchardCore.Scripting;

namespace OrchardCore.Tests.Scripting;

/// <summary>
/// Pins that <see cref="DefaultScriptingManager"/> ends the scope it opens for a directive. Nothing else
/// can: it creates one scope per evaluated expression and never hands it to the caller, so an engine that
/// wants to know when the evaluation is over can only learn it from here.
/// </summary>
public class DefaultScriptingManagerTests
{
    [Fact]
    public void Evaluate_WhenTheScriptSucceeds_DisposesTheScope()
    {
        var engine = new RecordingScriptingEngine();
        var manager = new DefaultScriptingManager([engine], []);

        Assert.Equal("evaluated", manager.Evaluate("test:anything", null, null, null));
        Assert.Equal(1, engine.Scope.DisposeCount);
    }

    [Fact]
    public void Evaluate_WhenTheScriptThrows_StillDisposesTheScope()
    {
        var engine = new RecordingScriptingEngine { Throw = true };
        var manager = new DefaultScriptingManager([engine], []);

        Assert.Throws<InvalidOperationException>(() => manager.Evaluate("test:anything", null, null, null));
        Assert.Equal(1, engine.Scope.DisposeCount);
    }

    [Fact]
    public async Task EvaluateAsync_WhenTheScriptSucceeds_DisposesTheScopeAfterTheEvaluationCompletes()
    {
        var engine = new RecordingScriptingEngine();
        var manager = new DefaultScriptingManager([engine], []);

        var result = await manager.EvaluateAsync("test:anything", null, null, null, TestContext.Current.CancellationToken);

        Assert.Equal("evaluated", result);

        // Not merely disposed, but disposed after the awaited evaluation had finished: an engine cannot be
        // reset while an asynchronous evaluation it started is still outstanding.
        Assert.Equal(1, engine.Scope.DisposeCount);
        Assert.True(engine.Scope.DisposedAfterEvaluation);
    }

    [Fact]
    public async Task EvaluateAsync_WhenTheScriptThrows_StillDisposesTheScope()
    {
        var engine = new RecordingScriptingEngine { Throw = true };
        var manager = new DefaultScriptingManager([engine], []);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.EvaluateAsync("test:anything", null, null, null, TestContext.Current.CancellationToken));

        Assert.Equal(1, engine.Scope.DisposeCount);
    }

    [Fact]
    public void Evaluate_WhenTheScopeIsNotDisposable_Succeeds()
    {
        // IScriptingScope is a marker interface, so an engine implemented outside this repository is under
        // no obligation to return something disposable.
        var manager = new DefaultScriptingManager([new PlainScriptingEngine()], []);

        Assert.Equal("evaluated", manager.Evaluate("plain:anything", null, null, null));
    }

    private sealed class RecordingScriptingEngine : IScriptingEngine
    {
        public RecordingScope Scope { get; } = new();

        public bool Throw { get; set; }

        public string Prefix => "test";

        public IScriptingScope CreateScope(IEnumerable<GlobalMethod> methods, IServiceProvider serviceProvider, IFileProvider fileProvider, string basePath)
            => Scope;

        public object Evaluate(IScriptingScope scope, string script)
        {
            if (Throw)
            {
                throw new InvalidOperationException("boom");
            }

            Scope.Evaluated = true;

            return "evaluated";
        }

        public async Task<object> EvaluateAsync(IScriptingScope scope, string script, CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            return Evaluate(scope, script);
        }
    }

    private sealed class RecordingScope : IScriptingScope, IDisposable
    {
        public int DisposeCount { get; private set; }

        public bool Evaluated { get; set; }

        public bool DisposedAfterEvaluation { get; private set; }

        public void Dispose()
        {
            DisposeCount++;
            DisposedAfterEvaluation = Evaluated;
        }
    }

    private sealed class PlainScriptingEngine : IScriptingEngine
    {
        public string Prefix => "plain";

        public IScriptingScope CreateScope(IEnumerable<GlobalMethod> methods, IServiceProvider serviceProvider, IFileProvider fileProvider, string basePath)
            => new PlainScope();

        public object Evaluate(IScriptingScope scope, string script) => "evaluated";

        private sealed class PlainScope : IScriptingScope;
    }
}
