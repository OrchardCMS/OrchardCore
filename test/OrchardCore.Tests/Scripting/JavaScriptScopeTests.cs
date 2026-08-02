using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;

namespace OrchardCore.Tests.Scripting;

public class JavaScriptScopeTests
{
    [Fact]
    public void RegisteredGlobals_AreNotBuiltWhenTheScriptDoesNotUseThem()
    {
        var (engine, methods, serviceProvider, provider) = CreateEngine();

        var scope = engine.CreateScope(methods, serviceProvider, null, null);

        Assert.Equal(0, provider.BuildCount);
        Assert.Equal(1, Convert.ToInt32(engine.Evaluate(scope, "return 1;")));
        Assert.Equal(0, provider.BuildCount);
    }

    [Fact]
    public void RegisteredGlobals_AreBuiltOncePerEngine()
    {
        var (engine, methods, serviceProvider, provider) = CreateEngine();

        var scope = engine.CreateScope(methods, serviceProvider, null, null);

        Assert.Equal("counted", engine.Evaluate(scope, "return counted();"));
        Assert.Equal(1, provider.BuildCount);

        // The materialized value is kept by the engine, so the identity of the global is stable.
        Assert.True((bool)engine.Evaluate(scope, "var first = counted; var second = counted; return first === second;"));
        Assert.Equal(1, provider.BuildCount);

        // A second scope gets its own engine, and therefore its own delegate built from its own services.
        var otherScope = engine.CreateScope(methods, serviceProvider, null, null);

        Assert.Equal("counted", engine.Evaluate(otherScope, "return counted();"));
        Assert.Equal(2, provider.BuildCount);
    }

    [Fact]
    public void RegisteredGlobals_AreNotEnumerable()
    {
        var (engine, methods, serviceProvider, _) = CreateEngine();

        var scope = engine.CreateScope(methods, serviceProvider, null, null);

        Assert.True((bool)engine.Evaluate(scope, "return Object.keys(globalThis).indexOf('counted') === -1;"));
        Assert.True((bool)engine.Evaluate(scope, "return 'counted' in globalThis;"));
    }

    [Fact]
    public void RegisteredGlobals_AreNotBuiltWhenTheScriptDeletesThemFirst()
    {
        var (engine, methods, serviceProvider, provider) = CreateEngine();

        var scope = engine.CreateScope(methods, serviceProvider, null, null);

        Assert.Equal("undefined", engine.Evaluate(scope, "delete globalThis.counted; return typeof counted;"));
        Assert.Equal(0, provider.BuildCount);
    }

    [Fact]
    public void ScopedMethods_TakePrecedenceOverRegisteredGlobalsOfTheSameName()
    {
        var (engine, methods, serviceProvider, provider) = CreateEngine();

        var scopedMethod = new GlobalMethod
        {
            Name = "counted",
            Method = _ => (Func<string>)(() => "scoped"),
        };

        var scope = engine.CreateScope(methods.Concat([scopedMethod]), serviceProvider, null, null);

        Assert.Equal("scoped", engine.Evaluate(scope, "return counted();"));
        Assert.Equal(0, provider.BuildCount);
    }

    [Fact]
    public async Task AsynchronousGlobals_AreBuiltOnlyWhenUsed()
    {
        var (engine, methods, serviceProvider, provider) = CreateEngine();

        var scope = engine.CreateScope(methods, serviceProvider, null, null);

        Assert.Equal("counted async", await engine.EvaluateAsync(scope, "return countedAsync();", TestContext.Current.CancellationToken));
        Assert.Equal(1, provider.AsyncBuildCount);

        // The synchronous variant of the same method was not touched.
        Assert.Equal(0, provider.BuildCount);
    }

    [Fact]
    public void RegisteredGlobals_AreBuiltWithTheServicesOfTheScopeThatOwnsTheEngine()
    {
        var (engine, methods) = CreateEngineWithScopeNames();

        // Both scopes are alive at the same time and share the engine that declared the global, so the
        // engine a global is materialized on is the only thing that can tell their services apart.
        var firstScope = engine.CreateScope(methods, CreateServices("first"), null, null);
        var secondScope = engine.CreateScope(methods, CreateServices("second"), null, null);

        Assert.Equal("second", engine.Evaluate(secondScope, "return scopeName();"));
        Assert.Equal("first", engine.Evaluate(firstScope, "return scopeName();"));
    }

    [Fact]
    public void RegisteredGlobals_AreBuiltWithTheServicesOfTheirOwnScope_WhenTwoEnginesAreAliveAtOnce()
    {
        // Each tenant has its own scripting engine, and the mapping from a Jint engine to the services it
        // was created for belongs to one of them, so a scope of one tenant must not be visible to the other.
        var (first, firstMethods) = CreateEngineWithScopeNames();
        var (second, secondMethods) = CreateEngineWithScopeNames();

        var firstScope = first.CreateScope(firstMethods, CreateServices("first"), null, null);
        var secondScope = second.CreateScope(secondMethods, CreateServices("second"), null, null);

        Assert.Equal("first", first.Evaluate(firstScope, "return scopeName();"));
        Assert.Equal("second", second.Evaluate(secondScope, "return scopeName();"));
    }

    [Fact]
    public async Task RegisteredGlobals_AreBuiltWithTheServicesOfTheScopeThatOwnsTheEngine_Asynchronously()
    {
        var (engine, methods) = CreateEngineWithScopeNames();

        var firstScope = engine.CreateScope(methods, CreateServices("first"), null, null);
        var secondScope = engine.CreateScope(methods, CreateServices("second"), null, null);

        Assert.Equal("second async", await engine.EvaluateAsync(secondScope, "return scopeNameAsync();", TestContext.Current.CancellationToken));
        Assert.Equal("first async", await engine.EvaluateAsync(firstScope, "return scopeNameAsync();", TestContext.Current.CancellationToken));
    }

    private static (IScriptingEngine Engine, IEnumerable<GlobalMethod> Methods) CreateEngineWithScopeNames()
    {
        var serviceProvider = new ServiceCollection()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine()
            .AddSingleton<IGlobalMethodProvider, ScopeNameMethodProvider>()
            .BuildServiceProvider();

        var scriptingManager = serviceProvider.GetRequiredService<IScriptingManager>();

        return (scriptingManager.GetScriptingEngine("js"), scriptingManager.GlobalMethodProviders.SelectMany(p => p.GetMethods()).ToArray());
    }

    private static ServiceProvider CreateServices(string scopeName)
        => new ServiceCollection()
            .AddSingleton(new ScopeName(scopeName))
            .BuildServiceProvider();

    private sealed record ScopeName(string Value);

    private sealed class ScopeNameMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _globalMethod = new()
        {
            Name = "scopeName",

            // The factory is handed the services of the scope the reading engine belongs to, so what it
            // captures is what proves which scope the global was built for.
            Method = sp => (Func<string>)(() => sp.GetRequiredService<ScopeName>().Value),
            AsyncMethod = sp => (Func<Task<string>>)(() => Task.FromResult(sp.GetRequiredService<ScopeName>().Value + " async")),
        };

        public IEnumerable<GlobalMethod> GetMethods() => [_globalMethod];
    }

    private static (IScriptingEngine Engine, IEnumerable<GlobalMethod> Methods, IServiceProvider ServiceProvider, CountingMethodProvider Provider) CreateEngine()
    {
        var provider = new CountingMethodProvider();

        var serviceProvider = new ServiceCollection()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine()
            .AddSingleton<IGlobalMethodProvider>(provider)
            .BuildServiceProvider();

        var scriptingManager = serviceProvider.GetRequiredService<IScriptingManager>();
        var engine = scriptingManager.GetScriptingEngine("js");
        var methods = scriptingManager.GlobalMethodProviders.SelectMany(p => p.GetMethods()).ToArray();

        return (engine, methods, serviceProvider, provider);
    }

    private sealed class CountingMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _globalMethod;

        public CountingMethodProvider()
        {
            _globalMethod = new GlobalMethod
            {
                Name = "counted",
                Method = _ =>
                {
                    BuildCount++;

                    return (Func<string>)(() => "counted");
                },
                AsyncMethod = _ =>
                {
                    AsyncBuildCount++;

                    return (Func<Task<string>>)(() => Task.FromResult("counted async"));
                },
            };
        }

        public int BuildCount { get; private set; }

        public int AsyncBuildCount { get; private set; }

        public IEnumerable<GlobalMethod> GetMethods() => [_globalMethod];
    }
}
