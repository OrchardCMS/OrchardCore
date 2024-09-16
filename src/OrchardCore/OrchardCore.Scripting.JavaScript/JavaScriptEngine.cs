using System.Text.Json.Dynamic;
using System.Text.Json.Nodes;
using Jint;
using Jint.Runtime.Interop;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Scripting.JavaScript;

public sealed class JavaScriptEngine : IScriptingEngine
{
    private readonly IMemoryCache _memoryCache;

    public JavaScriptEngine(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public string Prefix => "js";

    public IScriptingScope CreateScope(IEnumerable<GlobalMethod> methods, IServiceProvider serviceProvider, IFileProvider fileProvider, string basePath)
    {
        var engine = new Engine(options =>
        {
            options.SetWrapObjectHandler(static (e, target, type) =>
            {
                if (target is JsonDynamicObject dynamicObject)
                {
                    return ObjectWrapper.Create(e, (JsonObject)dynamicObject, type);
                }

                if (target is JsonDynamicArray dynamicArray)
                {
                    return ObjectWrapper.Create(e, (JsonArray)dynamicArray, type);
                }

                if (target is JsonDynamicValue dynamicValue)
                {
                    return ObjectWrapper.Create(e, (JsonValue)dynamicValue, type);
                }

                return ObjectWrapper.Create(e, target, type);
            });
        });

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
