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
            // The globals of the registered method providers are already declared on the engine by the
            // options, so nothing has to be declared for them here. Any other method, including one that
            // shadows a registered name, is declared on this engine and replaces the property the options
            // installed — which is what gives it precedence.
            var lazyGlobal = default(JavaScriptEngine.LazyGlobalMethod);

            if (lazyGlobals != null
                && lazyGlobals.TryGetValue(method.Name, out var candidate)
                && ReferenceEquals(candidate.Method, method))
            {
                lazyGlobal = candidate;
            }

            if (method.Method != null && !lazyGlobal.HasSyncGlobal)
            {
                AddLazyGlobal(method.Name, method.Method);
            }

            if (method.AsyncMethod != null && !lazyGlobal.HasAsyncGlobal)
            {
                AddLazyGlobal(method.Name + "Async", method.AsyncMethod);
            }
        }
    }

    public Engine Engine { get; }

    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Declares a global on this engine whose delegate is built the first time a script reads the name.
    /// </summary>
    /// <remarks>
    /// The engine-level counterpart of the declaration the options make for the registered providers. It is
    /// the one that can be used here, because the value depends on the services of this scope, which the
    /// shared options have no way to reach. Declaring rather than setting means a caller that supplies ten
    /// methods for a script that calls one pays for one.
    /// </remarks>
    private void AddLazyGlobal(string name, Func<IServiceProvider, Delegate> factory)
        => Engine.Advanced.AddLazyGlobal(
            name,
            (ServiceProvider, factory),
            static (engine, state) => JavaScriptEngine.CreateGlobal(engine, state.ServiceProvider, state.factory),
            JavaScriptEngine.GlobalPropertyFlags);

    /// <summary>
    /// Gets the services of the scope that owns the given engine.
    /// </summary>
    internal static bool TryGetServiceProvider(Engine engine, out IServiceProvider serviceProvider)
        => _engineServiceProviders.TryGetValue(engine, out serviceProvider);
}
