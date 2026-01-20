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
    }

    public string Prefix => "js";

    public IScriptingScope CreateScope(IEnumerable<GlobalMethod> methods, IServiceProvider serviceProvider, IFileProvider fileProvider, string basePath)
    {
        var engine = new Engine(_jintOptions);

        foreach (var method in methods)
        {
            engine.SetValue(method.Name, method.Method(serviceProvider));
        }

        return new JavaScriptScope(engine, serviceProvider);
    }

    public object Evaluate(IScriptingScope scope, string script)
    {
        static void ThrowInvalidScopeTypeException()
        {
            throw new ArgumentException($"Expected a scope of type {nameof(JavaScriptScope)}", nameof(scope));
        }

        if (scope is not JavaScriptScope jsScope)
        {
            ThrowInvalidScopeTypeException();
        }

        var parsedAst = _memoryCache.GetOrCreate(script, static entry => Engine.PrepareScript((string)entry.Key));

        var result = jsScope.Engine.Evaluate(parsedAst).ToObject();

        return result;
    }
}
