using Jint;

namespace OrchardCore.Scripting.JavaScript;

public class JavaScriptScope : IScriptingScope
{
    public JavaScriptScope(Engine engine, IServiceProvider serviceProvider, IEnumerable<GlobalMethod> methods)
        : this(engine, serviceProvider, methods, lazyGlobals: null, ownsEngine: false)
    {
    }

    internal JavaScriptScope(
        Engine engine,
        IServiceProvider serviceProvider,
        IEnumerable<GlobalMethod> methods,
        IReadOnlyDictionary<string, JavaScriptEngine.LazyGlobalMethod> lazyGlobals,
        bool ownsEngine)
    {
        Engine = engine;
        ServiceProvider = serviceProvider;

        // A lazily registered global is only materialized when a script reads it, which happens long after
        // the engine was built. The delegate it wraps is produced by a factory that takes the service
        // provider of the evaluation it belongs to, so the engine has to be able to find its scope back.
        // Jint reserves [[HostDefined]] on an engine for exactly this, so the engine carries the services
        // of the evaluation it is serving.
        //
        // An engine this scope was not given by JavaScriptEngine belongs to whoever built it, and that
        // slot is theirs. Claiming it while it is empty keeps a caller who wraps an engine of their own
        // working as before; refusing to claim it while it is in use means a registered global on such an
        // engine fails with the exception in JavaScriptEngine.CreateGlobal, rather than the caller's own
        // state being destroyed to make one work.
        if (ownsEngine || engine.Advanced.HostDefined is null)
        {
            engine.Advanced.HostDefined = serviceProvider;
        }

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
    /// <para>
    /// The services are handed to the factory as state rather than read back off the engine, so a method a
    /// caller supplied is always built from the scope that supplied it, whatever the engine happens to
    /// carry.
    /// </para>
    /// </remarks>
    private void AddLazyGlobal(string name, Func<IServiceProvider, Delegate> factory)
        => Engine.Advanced.AddLazyGlobal(
            name,
            (ServiceProvider, factory),
            static (engine, state) => JavaScriptEngine.CreateGlobal(engine, state.ServiceProvider, state.factory),
            JavaScriptEngine.GlobalPropertyFlags);
}
