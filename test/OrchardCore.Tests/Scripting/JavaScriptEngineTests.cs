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
}
