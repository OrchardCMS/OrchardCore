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
