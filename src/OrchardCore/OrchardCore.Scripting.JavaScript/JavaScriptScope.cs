using System.Runtime.CompilerServices;
using Jint;

namespace OrchardCore.Scripting.JavaScript;

public class JavaScriptScope : IScriptingScope
{
    // A lazily registered global is only materialized when a script reads it, which happens long after
    // the engine was built. The delegate it wraps is produced by a factory that takes the service
    // provider of the evaluation it belongs to, so the engine has to be able to find its scope back.
    // The table holds the engine weakly, so an entry disappears together with the engine that keys it.
    private static readonly ConditionalWeakTable<Engine, IServiceProvider> _engineServiceProviders = new();

    public JavaScriptScope(Engine engine, IServiceProvider serviceProvider, IEnumerable<GlobalMethod> methods)
        : this(engine, serviceProvider, methods, lazyGlobals: null)
    {
    }

    internal JavaScriptScope(
        Engine engine,
        IServiceProvider serviceProvider,
        IEnumerable<GlobalMethod> methods,
        IReadOnlyDictionary<string, JavaScriptEngine.LazyGlobalMethod> lazyGlobals)
    {
        Engine = engine;
        ServiceProvider = serviceProvider;

        _engineServiceProviders.AddOrUpdate(engine, serviceProvider);

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

    /// <summary>
    /// Gets the services of the scope that owns the given engine.
    /// </summary>
    internal static bool TryGetServiceProvider(Engine engine, out IServiceProvider serviceProvider)
        => _engineServiceProviders.TryGetValue(engine, out serviceProvider);
}
