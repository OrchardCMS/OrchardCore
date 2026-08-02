using Jint;
using JintOptions = Jint.Options;

namespace OrchardCore.Scripting.JavaScript;

/// <summary>
/// Keeps a bounded set of idle Jint engines so that consecutive evaluations of the same tenant can share
/// one, instead of paying for a new engine — and for re-declaring every registered global on it — per
/// evaluated expression.
/// </summary>
/// <remarks>
/// <para>
/// An engine is handed back to the pool only after its global bindings have been returned to the state they
/// were captured in right after construction, through Jint's <c>GlobalSnapshot</c>. That reverts the
/// variables and functions the evaluation declared, the eagerly installed per-scope methods, the top-level
/// <c>let</c>/<c>const</c>/<c>class</c> declarations, and the interop wrapper caches, and it puts the
/// registered globals back in their not-yet-created state so that the next evaluation builds them from its
/// own services. What it deliberately does not revert is anything a script did to the built-in prototypes,
/// or to an object graph reachable from a global it did not replace. Reuse is therefore confined to one
/// tenant, where scripts are authored at a single trust level; see the remarks on
/// <see cref="JavaScriptEngine"/>.
/// </para>
/// <para>
/// The pool never blocks and never limits concurrency. A rental that finds no idle engine builds one, and a
/// return that finds no free slot drops the engine on the floor for the garbage collector — exactly the
/// behavior of every evaluation before pooling existed. The size is therefore a bound on how much state is
/// <em>retained</em> between requests, not on how many evaluations can run at once.
/// </para>
/// </remarks>
internal sealed class JavaScriptEnginePool
{
    private readonly JintOptions _options;

    // One slot per pooled engine. A slot holds either an idle engine or null, and an engine is taken out of
    // (and put back into) a slot with a single interlocked operation, so it is the exchange itself that
    // hands the engine over: two callers can never come away with the same engine.
    private readonly PooledJavaScriptEngine[] _idle;

    // Set at most once, and only in the one direction. A realm whose global object resolves its properties
    // from outside the engine cannot be captured, and cannot become capturable later, so the first refusal
    // retires reuse for the lifetime of the tenant rather than being retried per evaluation.
    private volatile bool _resetUnsupported;

    internal JavaScriptEnginePool(JintOptions options, int size)
    {
        _options = options;
        _idle = new PooledJavaScriptEngine[size];
    }

    /// <summary>
    /// Gets an engine to evaluate on. The caller owns it until it passes the same rental to
    /// <see cref="Return"/>, and must not use it afterwards.
    /// </summary>
    internal PooledJavaScriptEngine Rent()
    {
        var idle = _idle;

        // Both ends scan from the lowest slot, which is what a tenant evaluating one expression at a time
        // wants: the engine it just gave back goes into slot zero and comes straight back out of it, so the
        // interpreter state that engine built for a script keeps paying off. Under real concurrency the
        // rentals spread over the slots and each engine warms up for itself.
        for (var i = 0; i < idle.Length; i++)
        {
            var candidate = Volatile.Read(ref idle[i]);

            if (candidate is not null && Interlocked.CompareExchange(ref idle[i], null, candidate) == candidate)
            {
                return candidate;
            }
        }

        return Create();
    }

    /// <summary>
    /// Resets the engine of the given rental and makes it available again. A rental must be returned at most
    /// once; <see cref="JavaScriptScope"/> guarantees that by handing the rental over with an interlocked
    /// exchange, so the engine cannot still be reachable from the caller when this runs.
    /// </summary>
    internal void Return(PooledJavaScriptEngine rental)
    {
        var engine = rental.Engine;
        var snapshot = rental.Snapshot;

        // The service provider of the evaluation that has just ended must not be reachable from an idle
        // engine, and must not be found by a global that somehow gets built on one. Detaching it turns a use
        // of a returned engine into an exception on the first registered global it reads, rather than a
        // delegate quietly built from another request's services.
        JavaScriptScope.DetachServiceProvider(engine);

        if (snapshot is null)
        {
            // This engine was built while resetting was known to be unsupported. It is not poolable and
            // there is nothing to undo, so let it go.
            return;
        }

        try
        {
            engine.Advanced.RestoreGlobalSnapshot(snapshot);
        }
        catch (Exception)
        {
            // The engine keeps whatever the evaluation left on it, so it must never be handed out again.
            // The realistic cause is a caller ending the scope while an asynchronous evaluation it started
            // is still outstanding, which Jint refuses to reset underneath. Dropping the engine is always
            // correct and costs only the reuse; rethrowing would turn a caller's timing mistake into a
            // failure of the disposal that is trying to clean up after it.
            return;
        }

        var idle = _idle;

        for (var i = 0; i < idle.Length; i++)
        {
            if (Volatile.Read(ref idle[i]) is null && Interlocked.CompareExchange(ref idle[i], rental, null) is null)
            {
                return;
            }
        }

        // Every slot is taken: more evaluations were in flight at once than the pool is sized for. Dropping
        // the engine is what keeps the size a real bound on retained state.
    }

    private PooledJavaScriptEngine Create()
    {
        var engine = new Engine(_options);

        if (_resetUnsupported)
        {
            return new PooledJavaScriptEngine(engine, snapshot: null);
        }

        try
        {
            // Captured before anything of an evaluation has touched the engine, so the registered globals
            // are recorded in their not-yet-created state and a reset puts them back that way. Capturing
            // does not create them.
            return new PooledJavaScriptEngine(engine, engine.Advanced.CaptureGlobalSnapshot());
        }
        catch (NotSupportedException)
        {
            // A host replaced the global object with one that stores its properties itself, so a snapshot
            // could not tell what to put back. Reuse is off; evaluation is unaffected.
            _resetUnsupported = true;

            return new PooledJavaScriptEngine(engine, snapshot: null);
        }
    }
}

/// <summary>
/// An engine and the snapshot of the global bindings it has to be returned to before it can be reused. The
/// snapshot is <see langword="null"/> for an engine that cannot be reset, which is how <see cref="JavaScriptEnginePool.Return"/>
/// knows to drop it.
/// </summary>
internal sealed class PooledJavaScriptEngine
{
    internal PooledJavaScriptEngine(Engine engine, GlobalSnapshot snapshot)
    {
        Engine = engine;
        Snapshot = snapshot;
    }

    internal Engine Engine { get; }

    internal GlobalSnapshot Snapshot { get; }
}
