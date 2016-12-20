using System;
using System.Collections.Generic;
using Jint;
using Microsoft.Extensions.Localization;

namespace Orchard.Scripting.JavaScript
{
    public class JavaScriptEngine : IScriptingEngine
    {
        public JavaScriptEngine(IStringLocalizer<JavaScriptEngine> localizer)
        {
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

            var result = jsScope.Engine.Execute(script).GetCompletionValue()?.ToObject();

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
