using OrchardCore.Scripting;

namespace OrchardCore.Tests.Scripting;

public class DefaultScriptingManagerTests
{
    [Fact]
    public void GetScriptingEngine_KnownPrefix_ReturnsTheEngine()
    {
        var js = new FakeScriptingEngine("js");
        var manager = CreateManager([new FakeScriptingEngine("csharp"), js]);

        Assert.Same(js, manager.GetScriptingEngine("js"));
    }

    [Fact]
    public void GetScriptingEngine_UnknownPrefix_ReturnsNull()
    {
        var manager = CreateManager([new FakeScriptingEngine("js")]);

        Assert.Null(manager.GetScriptingEngine("unknown"));
    }

    [Fact]
    public void GetScriptingEngine_PrefixDifferingInCase_ReturnsNull()
    {
        var manager = CreateManager([new FakeScriptingEngine("js")]);

        Assert.Null(manager.GetScriptingEngine("JS"));
    }

    [Fact]
    public void GetScriptingEngine_NullPrefix_ReturnsNull()
    {
        var manager = CreateManager([new FakeScriptingEngine("js")]);

        Assert.Null(manager.GetScriptingEngine(null));
    }

    [Fact]
    public void GetScriptingEngine_TwoEnginesSharingAPrefix_ReturnsTheFirstRegistration()
    {
        var first = new FakeScriptingEngine("js");
        var second = new FakeScriptingEngine("js");

        var manager = CreateManager([first, second]);

        Assert.Same(first, manager.GetScriptingEngine("js"));
    }

    [Fact]
    public void Evaluate_DirectiveWithoutAPrefix_ReturnsTheDirective()
    {
        var manager = CreateManager([new FakeScriptingEngine("js")]);

        Assert.Equal("no prefix here", manager.Evaluate("no prefix here", null, null, null));
    }

    [Fact]
    public void Evaluate_DirectiveWithAnUnknownPrefix_ReturnsTheDirective()
    {
        var manager = CreateManager([new FakeScriptingEngine("js")]);

        Assert.Equal("unknown:1 + 1", manager.Evaluate("unknown:1 + 1", null, null, null));
    }

    [Fact]
    public void Evaluate_WithoutScopedProviders_PassesTheRegisteredMethods()
    {
        var engine = new FakeScriptingEngine("js");
        var manager = CreateManager([engine], [new FakeMethodProvider("registered")]);

        Assert.Equal("1 + 1", manager.Evaluate("js:1 + 1", null, null, null));
        Assert.Equal(["registered"], engine.LastMethods.Select(method => method.Name));
    }

    [Fact]
    public void Evaluate_WithScopedProviders_PassesTheScopedMethodsAfterTheRegisteredOnes()
    {
        var engine = new FakeScriptingEngine("js");
        var manager = CreateManager([engine], [new FakeMethodProvider("registered")]);

        manager.Evaluate("js:1 + 1", null, null, [new FakeMethodProvider("scoped")]);

        // A scope installs the methods in the order it receives them, so the scoped ones have to come last
        // for one of them to be able to shadow a registered method of the same name.
        Assert.Equal(["registered", "scoped"], engine.LastMethods.Select(method => method.Name));
    }

    [Fact]
    public void Evaluate_TwiceWithDifferentScopedProviders_PassesOnlyTheProvidersOfEachEvaluation()
    {
        var engine = new FakeScriptingEngine("js");
        var manager = CreateManager([engine], [new FakeMethodProvider("registered")]);

        manager.Evaluate("js:1 + 1", null, null, [new FakeMethodProvider("first")]);
        manager.Evaluate("js:1 + 1", null, null, [new FakeMethodProvider("second")]);

        Assert.Equal(["registered", "second"], engine.LastMethods.Select(method => method.Name));
    }

    [Fact]
    public async Task EvaluateAsync_WithoutScopedProviders_PassesTheRegisteredMethods()
    {
        var engine = new FakeScriptingEngine("js");
        var manager = CreateManager([engine], [new FakeMethodProvider("registered")]);

        Assert.Equal("1 + 1", await manager.EvaluateAsync("js:1 + 1", null, null, null, TestContext.Current.CancellationToken));
        Assert.Equal(["registered"], engine.LastMethods.Select(method => method.Name));
    }

    [Fact]
    public async Task EvaluateAsync_WithScopedProviders_PassesTheScopedMethodsAfterTheRegisteredOnes()
    {
        var engine = new FakeScriptingEngine("js");
        var manager = CreateManager([engine], [new FakeMethodProvider("registered")]);

        await manager.EvaluateAsync("js:1 + 1", null, null, [new FakeMethodProvider("scoped")], TestContext.Current.CancellationToken);

        Assert.Equal(["registered", "scoped"], engine.LastMethods.Select(method => method.Name));
    }

    [Fact]
    public async Task EvaluateAsync_DirectiveWithAnUnknownPrefix_ReturnsTheDirective()
    {
        var manager = CreateManager([new FakeScriptingEngine("js")]);

        Assert.Equal("unknown:1 + 1", await manager.EvaluateAsync("unknown:1 + 1", null, null, null, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Evaluate_WhenTheScriptSucceeds_DisposesTheScope()
    {
        // The manager opens a scope per evaluated directive and never hands it to the caller, so an engine
        // that wants to know when the evaluation is over - the JavaScript engine reuses its engines between
        // evaluations - can only learn it from here.
        var engine = new RecordingScriptingEngine();
        var manager = CreateManager([engine]);

        Assert.Equal("evaluated", manager.Evaluate("test:anything", null, null, null));
        Assert.Equal(1, engine.Scope.DisposeCount);
    }

    [Fact]
    public void Evaluate_WhenTheScriptThrows_StillDisposesTheScope()
    {
        var engine = new RecordingScriptingEngine { Throw = true };
        var manager = CreateManager([engine]);

        Assert.Throws<InvalidOperationException>(() => manager.Evaluate("test:anything", null, null, null));
        Assert.Equal(1, engine.Scope.DisposeCount);
    }

    [Fact]
    public async Task EvaluateAsync_WhenTheScriptSucceeds_DisposesTheScopeAfterTheEvaluationCompletes()
    {
        var engine = new RecordingScriptingEngine();
        var manager = CreateManager([engine]);

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
        var manager = CreateManager([engine]);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.EvaluateAsync("test:anything", null, null, null, TestContext.Current.CancellationToken));

        Assert.Equal(1, engine.Scope.DisposeCount);
    }

    [Fact]
    public void Evaluate_WhenTheScopeIsNotDisposable_Succeeds()
    {
        // IScriptingScope is a marker interface, so an engine implemented outside this repository is under
        // no obligation to return something disposable. FakeScriptingEngine's scope is not.
        var manager = CreateManager([new FakeScriptingEngine("js")]);

        Assert.Equal("1 + 1", manager.Evaluate("js:1 + 1", null, null, null));
    }

    private static DefaultScriptingManager CreateManager(
        IEnumerable<IScriptingEngine> engines,
        IEnumerable<IGlobalMethodProvider> globalMethodProviders = null)
        => new(engines, globalMethodProviders ?? []);

    private sealed class FakeScriptingEngine : IScriptingEngine
    {
        public FakeScriptingEngine(string prefix)
        {
            Prefix = prefix;
        }

        public string Prefix { get; }

        public GlobalMethod[] LastMethods { get; private set; }

        public IScriptingScope CreateScope(IEnumerable<GlobalMethod> methods, IServiceProvider serviceProvider, IFileProvider fileProvider, string basePath)
        {
            LastMethods = methods.ToArray();

            return new FakeScriptingScope();
        }

        public object Evaluate(IScriptingScope scope, string script) => script;

        private sealed class FakeScriptingScope : IScriptingScope
        {
        }
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

    private sealed class FakeMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod[] _methods;

        public FakeMethodProvider(params string[] names)
        {
            _methods = names.Select(name => new GlobalMethod { Name = name }).ToArray();
        }

        public IEnumerable<GlobalMethod> GetMethods() => _methods;
    }
}
