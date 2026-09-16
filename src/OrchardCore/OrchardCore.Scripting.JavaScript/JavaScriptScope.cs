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
