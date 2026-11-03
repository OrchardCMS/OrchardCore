using Jint;

namespace OrchardCore.Scripting.JavaScript;

public class JavaScriptScope : IScriptingScope, IDisposable
{
    /// <summary>
    /// What a scope created without services records in the engine's [[HostDefined]] slot. An empty slot
    /// means no scope was created for the engine at all, so a scope that has no services cannot leave the
    /// slot empty and has to say so with a value of its own.
    /// </summary>
    internal static readonly object NoServices = new();

    private readonly Engine _engine;
    private readonly JavaScriptEnginePool _pool;

    // The rental this scope has to give back, or null when the engine is not pooled or has already been
    // given back. Exchanged rather than assigned, so that disposing twice returns the engine once.
    private PooledJavaScriptEngine _rental;

    private volatile bool _disposed;

    public JavaScriptScope(Engine engine, IServiceProvider serviceProvider, IEnumerable<GlobalMethod> methods)
        : this(engine, serviceProvider, methods, lazyGlobals: null, ownsEngine: false, pool: null, rental: null)
    {
    }

    internal JavaScriptScope(
        Engine engine,
        IServiceProvider serviceProvider,
        IEnumerable<GlobalMethod> methods,
        IReadOnlyDictionary<string, JavaScriptEngine.LazyGlobalMethod> lazyGlobals,
        bool ownsEngine,
        JavaScriptEnginePool pool,
        PooledJavaScriptEngine rental)
    {
        _engine = engine;
        _pool = pool;
        _rental = rental;

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
            engine.Advanced.HostDefined = serviceProvider ?? NoServices;
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
                _engine.SetValue(method.Name, method.Method(ServiceProvider));
            }

            if (method.AsyncMethod != null && !lazyGlobal.HasAsyncGlobal)
            {
                _engine.SetValue(method.Name + "Async", method.AsyncMethod(ServiceProvider));
            }
        }
    }

    /// <summary>
    /// Gets the engine this scope evaluates on.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// The scope has been disposed. The engine may already be evaluating something else, so reaching it
    /// through a scope that has given it up is reported rather than allowed.
    /// </exception>
    public Engine Engine
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            return _engine;
        }
    }

    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Ends the scope, releasing the engine for reuse by a later evaluation of the same tenant.
    /// </summary>
    /// <remarks>
    /// Disposing more than once is safe and releases the engine once. Not disposing at all is safe too: the
    /// engine is simply never reused, which is what every evaluation did before pooling existed. That is the
    /// deliberate failure mode — an engine is single-threaded, so a scope that let go of one it might still
    /// be using would be far worse than one that keeps an engine to itself forever.
    /// </remarks>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _disposed = true;

        // The exchange, not the flag, is what makes the release happen exactly once: two threads disposing
        // at the same time both set the flag, and only one of them comes away with the rental.
        var rental = Interlocked.Exchange(ref _rental, null);

        if (rental != null)
        {
            _pool.Return(rental);
        }
    }
}
