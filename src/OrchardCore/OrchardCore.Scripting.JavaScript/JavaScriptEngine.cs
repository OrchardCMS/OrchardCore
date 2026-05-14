using Jint;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using JintOptions = Jint.Options;

namespace OrchardCore.Scripting.JavaScript;

public sealed class JavaScriptEngine : IScriptingEngine
{
    private readonly IMemoryCache _memoryCache;
    private readonly JintOptions _jintOptions;

    public JavaScriptEngine(IMemoryCache memoryCache, IOptions<JintOptions> jintOptions)
    {
        _memoryCache = memoryCache;
        _jintOptions = jintOptions.Value;
        _jintOptions.ExperimentalFeatures |= ExperimentalFeature.TaskInterop;
    }

    public string Prefix => "js";

    public IScriptingScope CreateScope(IEnumerable<GlobalMethod> methods, IServiceProvider serviceProvider, IFileProvider fileProvider, string basePath)
    {
        var engine = new Engine(_jintOptions);

        return new JavaScriptScope(engine, serviceProvider, methods);
    }

    public object Evaluate(IScriptingScope scope, string script)
    {
        var jsScope = GetJavaScriptScope(scope);
        jsScope.UseSyncMethods();

        var parsedAst = _memoryCache.GetOrCreate(script, static entry => Engine.PrepareScript((string)entry.Key));

        var result = jsScope.Engine.Evaluate(parsedAst).ToObject();

        return result;
    }

    public async Task<object> EvaluateAsync(IScriptingScope scope, string script, CancellationToken cancellationToken = default)
    {
        var jsScope = GetJavaScriptScope(scope);
        jsScope.UseAsyncMethods();

        var parsedAst = _memoryCache.GetOrCreate(script, static entry => Engine.PrepareScript((string)entry.Key));

        var result = await jsScope.Engine.EvaluateAsync(parsedAst, cancellationToken);

        return result.ToObject();
    }

    private static JavaScriptScope GetJavaScriptScope(IScriptingScope scope)
    {
        if (scope is not JavaScriptScope jsScope)
        {
            throw new ArgumentException($"Expected a scope of type {nameof(JavaScriptScope)}", nameof(scope));
        }

        return jsScope;
    }
}
