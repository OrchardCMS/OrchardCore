using System.Runtime.CompilerServices;
using Jint;

namespace OrchardCore.Scripting.JavaScript;

public class JavaScriptScope : IScriptingScope
{
    public JavaScriptScope(Engine engine, IServiceProvider serviceProvider, IEnumerable<GlobalMethod> methods)
        : this(engine, serviceProvider, methods, lazyGlobals: null, engineServiceProviders: null)
    {
    }

    internal JavaScriptScope(
        Engine engine,
        IServiceProvider serviceProvider,
        IEnumerable<GlobalMethod> methods,
        IReadOnlyDictionary<string, JavaScriptEngine.LazyGlobalMethod> lazyGlobals,
        ConditionalWeakTable<Engine, IServiceProvider> engineServiceProviders)
    {
        Engine = engine;
        ServiceProvider = serviceProvider;

        // Only the scripting engine that declared the lazy globals can materialize one, and the table it
        // hands over here is how it finds this scope back when a script eventually reads such a global. A
        // scope a caller builds around its own engine has no lazy global to serve, so it records nothing.
        engineServiceProviders?.AddOrUpdate(engine, serviceProvider);

        foreach (var method in methods)
        {
            // The globals of the registered method providers are already installed on the engine as lazy
            // properties, so nothing has to be created for them here. Any other method, including one that
            // shadows a registered name, is set eagerly and replaces the lazy property.
            var lazyGlobal = default(JavaScriptEngine.LazyGlobalMethod);

            if (lazyGlobals != null
                && lazyGlobals.TryGetValue(method.Name, out var candidate)
                && ReferenceEquals(candidate.Method, method))
            {
                lazyGlobal = candidate;
            }

            if (method.Method != null && !lazyGlobal.HasSyncGlobal)
            {
                Engine.SetValue(method.Name, method.Method(ServiceProvider));
            }

            if (method.AsyncMethod != null && !lazyGlobal.HasAsyncGlobal)
            {
                Engine.SetValue(method.Name + "Async", method.AsyncMethod(ServiceProvider));
            }
        }
    }

    public Engine Engine { get; }

    public IServiceProvider ServiceProvider { get; }
}
