using Acornima.Ast;
using Jint;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using JintOptions = Jint.Options;

namespace OrchardCore.Scripting.JavaScript;

public sealed class JavaScriptEngine : IScriptingEngine
{
    private static readonly MemoryCacheEntryOptions ScriptCacheEntryOptions = new MemoryCacheEntryOptions()
        .SetSlidingExpiration(TimeSpan.FromMinutes(30))
        ;

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

        var result = jsScope.Engine.Evaluate(PrepareScript(script)).ToObject();

        return result;
    }

    public async Task<object> EvaluateAsync(IScriptingScope scope, string script, CancellationToken cancellationToken = default)
    {
        var jsScope = GetJavaScriptScope(scope);

        var result = await jsScope.Engine.EvaluateAsync(PrepareScript(script), cancellationToken);

        return result.ToObject();
    }

    private Prepared<Script> PrepareScript(string script)
        => _memoryCache.GetOrCreate(
            new PreparedScriptCacheKey(script),
            static entry => Engine.PrepareScript(((PreparedScriptCacheKey)entry.Key).Script),
            ScriptCacheEntryOptions);

    private static JavaScriptScope GetJavaScriptScope(IScriptingScope scope)
    {
        if (scope is not JavaScriptScope jsScope)
        {
            throw new ArgumentException($"Expected a scope of type {nameof(JavaScriptScope)}", nameof(scope));
        }

        return jsScope;
    }

    /// <summary>
    /// Namespaces the prepared script entries stored in the shared <see cref="IMemoryCache"/>. Using a
    /// dedicated key type instead of the raw script text guarantees that a key registered by another
    /// component cannot be read back as a prepared script.
    /// </summary>
    private readonly record struct PreparedScriptCacheKey(string Script);
}
