using System;
using System.Collections.Generic;
using Jint;
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
            var engine = new Engine();

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

            var parsedAst = _memoryCache.GetOrCreate(script, static entry => Engine.PrepareScript((string) entry.Key));

            var result = jsScope.Engine.Evaluate(parsedAst).ToObject();

            return result;
        }
    }
}
