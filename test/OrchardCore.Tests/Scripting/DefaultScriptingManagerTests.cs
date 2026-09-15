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
