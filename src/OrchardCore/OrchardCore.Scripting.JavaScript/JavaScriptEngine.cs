using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Nodes;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Scripting.JavaScript
{
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
                // Make JsonArray behave like JS array.
                options.SetWrapObjectHandler(static (e, target, type) =>
                {
                    if (target is JsonArray)
                    {
                        var wrapped = new ObjectWrapper(e, target)
                        {
                            Prototype = e.Intrinsics.Array.PrototypeObject
                        };
                        return wrapped;
                    }

                    return new ObjectWrapper(e, target);
                });

                options.AddObjectConverter<JsonValueConverter>();

                // We cannot access this[string] with anything else than JsonObject, otherwise itw will throw.
                options.SetTypeResolver(new TypeResolver
                {
                    MemberFilter = static info =>
                    {
                        if (info.ReflectedType != typeof(JsonObject) && info.Name == "Item" && info is PropertyInfo p)
                        {
                            var parameters = p.GetIndexParameters();
                            return parameters.Length != 1 || parameters[0].ParameterType != typeof(string);
                        }

                        return true;
                    }
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
}
