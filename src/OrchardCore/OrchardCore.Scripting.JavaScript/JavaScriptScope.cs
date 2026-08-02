using System.Runtime.CompilerServices;
using Jint;

namespace OrchardCore.Scripting.JavaScript;

public class JavaScriptScope : IScriptingScope, IDisposable
{
    // A lazily registered global is only materialized when a script reads it, which happens long after
    // the engine was built. The delegate it wraps is produced by a factory that takes the service
    // provider of the evaluation it belongs to, so the engine has to be able to find its scope back.
    // The table holds the engine weakly, so an entry disappears together with the engine that keys it.
    private static readonly ConditionalWeakTable<Engine, IServiceProvider> _engineServiceProviders = new();

    private readonly Engine _engine;
    private readonly JavaScriptEnginePool _pool;

    // The rental this scope has to give back, or null when the engine is not pooled or has already been
    // given back. Exchanged rather than assigned, so that disposing twice returns the engine once.
    private PooledJavaScriptEngine _rental;

    private volatile bool _disposed;

    public JavaScriptScope(Engine engine, IServiceProvider serviceProvider, IEnumerable<GlobalMethod> methods)
        : this(engine, serviceProvider, methods, lazyGlobals: null, pool: null, rental: null)
    {
    }

    internal JavaScriptScope(
        Engine engine,
        IServiceProvider serviceProvider,
        IEnumerable<GlobalMethod> methods,
        IReadOnlyDictionary<string, JavaScriptEngine.LazyGlobalMethod> lazyGlobals,
        JavaScriptEnginePool pool,
        PooledJavaScriptEngine rental)
    {
        _engine = engine;
        _pool = pool;
        _rental = rental;

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

    /// <summary>
    /// Gets the services of the scope that owns the given engine.
    /// </summary>
    internal static bool TryGetServiceProvider(Engine engine, out IServiceProvider serviceProvider)
        => _engineServiceProviders.TryGetValue(engine, out serviceProvider);

    /// <summary>
    /// Forgets the services associated with an engine whose scope has ended, so that neither the engine nor
    /// anything it later builds can reach the service provider of a finished evaluation.
    /// </summary>
    internal static void DetachServiceProvider(Engine engine)
        => _engineServiceProviders.Remove(engine);
}
