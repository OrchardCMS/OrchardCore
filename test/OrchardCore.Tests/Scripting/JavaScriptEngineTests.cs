using System.Reflection;
using Jint;
using JintOptions = Jint.Options;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;

namespace OrchardCore.Tests.Scripting;

public class JavaScriptEngineTests
{
    [Fact]
    public void Evaluate_WhenTheScriptTextIsAlreadyUsedAsACacheKey_StillEvaluatesTheScript()
    {
        var services = new ServiceCollection()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine();

        var serviceProvider = services.BuildServiceProvider();

        const string script = "return 1 + 1;";

        // Another component of the application happens to use the same string as a cache key in the
        // shared memory cache. Prepared scripts have to be namespaced so that they cannot collide.
        serviceProvider.GetRequiredService<IMemoryCache>().Set(script, "an unrelated value");

        var engine = serviceProvider.GetServices<IScriptingEngine>().First(engine => engine.Prefix == "js");
        var scope = engine.CreateScope([], serviceProvider, null, null);

        Assert.Equal(2, Convert.ToInt32(engine.Evaluate(scope, script)));
    }

    [Fact]
    public void CreateScope_ByDefault_GivesTheEngineAnExecutionTimeout()
    {
        var serviceProvider = new ServiceCollection()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine()
            .BuildServiceProvider();

        var scope = (JavaScriptScope)CreateScope(serviceProvider);

        // The constraint types of Jint are internal, so the registration is asserted on the name of the
        // constraint the engine was built with rather than on its type.
        Assert.Contains(GetConstraints(scope.Engine), constraint => constraint.GetType().Name == "TimeConstraint");
    }

    [Fact]
    public void Evaluate_WhenAScriptExceedsTheConfiguredTimeout_Throws()
    {
        var services = new ServiceCollection()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine();

        // The default is registered by AddJavaScriptEngine(), so a later configuration replaces it.
        services.Configure<JintOptions>(options => options.TimeoutInterval(TimeSpan.FromMilliseconds(100)));

        var serviceProvider = services.BuildServiceProvider();

        var engine = GetJavaScriptEngine(serviceProvider);
        var scope = engine.CreateScope([], serviceProvider, null, null);

        Assert.Throws<TimeoutException>(() => engine.Evaluate(scope, "while (true) { }"));
    }

    [Fact]
    public void Evaluate_WhenAScriptExceedsTheConfiguredSettingsTimeout_Throws()
    {
        var services = new ServiceCollection()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine();

        // What binding the OrchardCore_Scripting_JavaScript configuration section produces.
        services.Configure<JavaScriptEngineOptions>(options => options.TimeoutInterval = TimeSpan.FromMilliseconds(100));

        var serviceProvider = services.BuildServiceProvider();

        var engine = GetJavaScriptEngine(serviceProvider);
        var scope = engine.CreateScope([], serviceProvider, null, null);

        Assert.Throws<TimeoutException>(() => engine.Evaluate(scope, "while (true) { }"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateScope_WhenTheConfiguredSettingsTimeoutIsNotPositive_GivesTheEngineNoExecutionTimeout(int seconds)
    {
        var services = new ServiceCollection()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine();

        services.Configure<JavaScriptEngineOptions>(options => options.TimeoutInterval = TimeSpan.FromSeconds(seconds));

        var scope = (JavaScriptScope)CreateScope(services.BuildServiceProvider());

        Assert.DoesNotContain(GetConstraints(scope.Engine), constraint => constraint.GetType().Name == "TimeConstraint");
    }

    [Fact]
    public void CreateScope_WhenTheTimeoutIsRemoved_GivesTheEngineNoExecutionTimeout()
    {
        var services = new ServiceCollection()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine();

        // TimeSpan.MaxValue removes the constraint rather than pushing the deadline out.
        services.Configure<JintOptions>(options => options.TimeoutInterval(TimeSpan.MaxValue));

        var scope = (JavaScriptScope)CreateScope(services.BuildServiceProvider());

        Assert.DoesNotContain(GetConstraints(scope.Engine), constraint => constraint.GetType().Name == "TimeConstraint");
    }

    private static IScriptingScope CreateScope(IServiceProvider serviceProvider)
        => GetJavaScriptEngine(serviceProvider).CreateScope([], serviceProvider, null, null);

    private static IScriptingEngine GetJavaScriptEngine(IServiceProvider serviceProvider)
        => serviceProvider.GetServices<IScriptingEngine>().First(engine => engine.Prefix == "js");

    private static IEnumerable<object> GetConstraints(Engine engine)
        => (IEnumerable<object>)typeof(Engine)
            .GetField("_constraints", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(engine);
}
