using System;
using System.Collections.Generic;
using Jint;
using Jint.Parser;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;

namespace Orchard.Scripting.JavaScript
{
    public class JavaScriptEngine : IScriptingEngine
    {
		private readonly IMemoryCache _memoryCache;

		public JavaScriptEngine(
			IMemoryCache memoryCache,
			IStringLocalizer<JavaScriptEngine> localizer)
        {
			_memoryCache = memoryCache;
            S = localizer;
        }

        IStringLocalizer S { get; }

        public LocalizedString Name => S["JavaScript"];

        public string Prefix => "js";

        public IScriptingScope CreateScope(IEnumerable<GlobalMethod> methods, IServiceProvider serviceProvider)
        {
            var engine = new Engine();
            
            foreach(var method in methods)
            {
                engine.SetValue(method.Name, method.Method(serviceProvider));
            }

            return new JavaScriptScope(engine, serviceProvider);
        }

        public object Evaluate(IScriptingScope scope, string script)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            var jsScope = scope as JavaScriptScope;

            if (jsScope == null)
            {
                throw new ArgumentException($"Expected a scope of type {nameof(JavaScriptScope)}", nameof(scope));
            }

			var parsedAst = _memoryCache.GetOrCreate(script, entry =>
			{
				var parser = new JavaScriptParser();
				return parser.Parse(script);
			});

			var result = jsScope.Engine.Execute(parsedAst).GetCompletionValue()?.ToObject();

            return result;
        }
    }

    public class MethodProxy
    {        
        public IList<object> Arguments { get; set; }
        public Func<IServiceProvider, IList<object>, object> Callback { get; set; }
        public object Invoke()
        {
            return Callback(null, Arguments);
        }
    }
}
