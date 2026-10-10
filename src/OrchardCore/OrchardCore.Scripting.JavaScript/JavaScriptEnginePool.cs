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
/// <para>
/// The interpreter state an engine builds while running a script is what makes reuse fast, and it is also
/// what keeps a finished evaluation's objects reachable: a warmed call site remembers the function it last
/// called, and the function a registered global resolves to holds the services of the request that read it.
/// So that state is discarded once it is <see cref="InterpreterCacheLifetime"/> old — when the engine is
/// returned after that, or by a sweep if it is sitting idle by then — and the next run of each script on
/// that engine builds it again. A sweep only ever touches an engine it has taken out of its slot, by the
/// same exchange a rental uses, so it can never reach an engine an evaluation is using.
/// </para>
/// </remarks>
internal sealed class JavaScriptEnginePool
{
    /// <summary>
    /// How long an engine keeps the interpreter state it has built before the pool discards it. An engine in
    /// constant use pays for one rebuild of each script it runs per period; an engine that goes idle stops
    /// keeping a finished request's objects reachable within twice this period.
    /// </summary>
    internal static readonly TimeSpan InterpreterCacheLifetime = TimeSpan.FromMinutes(1);

    private readonly JintOptions _options;
    private readonly TimeProvider _timeProvider;

    // Armed only while an idle engine still holds interpreter state, and then at most once per lifetime, so
    // a tenant that has stopped evaluating scripts has no timer running for it.
    private readonly ITimer _sweepTimer;
    private int _sweepScheduled;

    // One slot per pooled engine. A slot holds either an idle engine or null, and an engine is taken out of
    // (and put back into) a slot with a single interlocked operation, so it is the exchange itself that
    // hands the engine over: two callers can never come away with the same engine.
    private readonly PooledJavaScriptEngine[] _idle;

    // Set at most once, and only in the one direction. A realm whose global object resolves its properties
    // from outside the engine cannot be captured, and cannot become capturable later, so the first refusal
    // retires reuse for the lifetime of the tenant rather than being retried per evaluation.
    private volatile bool _resetUnsupported;

    internal JavaScriptEnginePool(JintOptions options, int size, TimeProvider timeProvider)
    {
        _options = options;
        _idle = new PooledJavaScriptEngine[size];
        _timeProvider = timeProvider;

        // The pool is built during whichever request first evaluates a script, and a timer would otherwise
        // capture that request's execution context — and with it the async-local state of the request and
        // its tenant scope — for as long as the timer lives. The timer only holds the pool weakly, so a
        // scheduled sweep does not keep a released tenant's engines alive either.
        var restoreFlow = !ExecutionContext.IsFlowSuppressed();

        if (restoreFlow)
        {
            ExecutionContext.SuppressFlow();
        }

        try
        {
            _sweepTimer = timeProvider.CreateTimer(
                static state =>
                {
                    if (((WeakReference<JavaScriptEnginePool>)state).TryGetTarget(out var pool))
                    {
                        pool.Sweep();
                    }
                },
                new WeakReference<JavaScriptEnginePool>(this),
                Timeout.InfiniteTimeSpan,
                Timeout.InfiniteTimeSpan);
        }
        finally
        {
            if (restoreFlow)
            {
                ExecutionContext.RestoreFlow();
            }
        }
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

        // The service provider of the evaluation that has just ended must not be found by a global that
        // somehow gets built on an idle engine, nor be reachable from one through the engine itself. (A
        // call site that has run warm still remembers the global it last called, and through it that
        // request's services, until the interpreter state is discarded below or by a sweep.) The scope
        // stored it in the engine's [[HostDefined]] slot, which a snapshot does not cover, so it is cleared
        // here. That turns a use of a returned engine into an exception on the first registered global it
        // reads (see JavaScriptEngine.CreateGlobal), rather than a delegate quietly built from another
        // request's services.
        engine.Advanced.HostDefined = null;

        if (snapshot is null)
        {
            // This engine was built while resetting was known to be unsupported. It is not poolable and
            // there is nothing to undo, so let it go.
            return;
        }

        try
        {
            engine.Advanced.RestoreGlobalSnapshot(snapshot);

            var now = _timeProvider.GetTimestamp();

            if (!rental.HoldsInterpreterState)
            {
                rental.HoldsInterpreterState = true;
                rental.InterpreterStateSince = now;
            }
            else if (IsExpired(rental, now))
            {
                // Done here as well as by the sweep: an engine that is always busy is never idle when a sweep
                // looks, and would otherwise keep the objects of call sites that have not run again since.
                engine.Advanced.DiscardInterpreterCaches();
                rental.HoldsInterpreterState = false;
            }
        }
        catch (Exception)
        {
            // The engine keeps whatever the evaluation left on it, so it must never be handed out again.
            // The realistic cause is a caller ending the scope while an asynchronous evaluation it started
            // is still outstanding, which Jint refuses to reset (or discard) underneath. Dropping the engine
            // is always correct and costs only the reuse; rethrowing would turn a caller's timing mistake
            // into a failure of the disposal that is trying to clean up after it.
            return;
        }

        // Read before the engine is back in a slot, after which it is no longer this caller's to read.
        var holdsInterpreterState = rental.HoldsInterpreterState;

        if (TryPutBack(rental) && holdsInterpreterState)
        {
            ScheduleSweep();
        }
    }

    /// <summary>
    /// Discards the interpreter state of the idle engines that have held it for
    /// <see cref="InterpreterCacheLifetime"/>. Runs on the sweep timer.
    /// </summary>
    private void Sweep()
    {
        // Cleared before the slots are read: an engine returned while this runs is either seen below or
        // schedules the next sweep itself.
        Interlocked.Exchange(ref _sweepScheduled, 0);

        var idle = _idle;
        var now = _timeProvider.GetTimestamp();
        var stillHeld = false;

        for (var i = 0; i < idle.Length; i++)
        {
            var candidate = Volatile.Read(ref idle[i]);

            // Read without owning the engine, so only a hint; it is checked again once the engine is ours.
            if (candidate is null || !candidate.HoldsInterpreterState)
            {
                continue;
            }

            if (!IsExpired(candidate, now))
            {
                stillHeld = true;

                continue;
            }

            // Taking the engine out of its slot is what makes it this sweep's: no rental can have it until it
            // is put back. If a rental got there first, its return applies the same rule.
            if (Interlocked.CompareExchange(ref idle[i], null, candidate) != candidate)
            {
                continue;
            }

            if (candidate.HoldsInterpreterState && IsExpired(candidate, now))
            {
                try
                {
                    candidate.Engine.Advanced.DiscardInterpreterCaches();
                    candidate.HoldsInterpreterState = false;
                }
                catch (Exception)
                {
                    // Not expected of an engine nothing is evaluating on, but an engine in an unknown state is
                    // dropped rather than handed out again.
                    continue;
                }
            }
            else
            {
                // Rented, used and returned between the read above and the exchange.
                stillHeld |= candidate.HoldsInterpreterState;
            }

            TryPutBack(candidate);
        }

        if (stillHeld)
        {
            ScheduleSweep();
        }
    }

    private bool TryPutBack(PooledJavaScriptEngine rental)
    {
        var idle = _idle;

        for (var i = 0; i < idle.Length; i++)
        {
            if (Volatile.Read(ref idle[i]) is null && Interlocked.CompareExchange(ref idle[i], rental, null) is null)
            {
                return true;
            }
        }

        // Every slot is taken: more evaluations were in flight at once than the pool is sized for. Dropping
        // the engine is what keeps the size a real bound on retained state.
        return false;
    }

    private void ScheduleSweep()
    {
        if (Volatile.Read(ref _sweepScheduled) == 0 && Interlocked.CompareExchange(ref _sweepScheduled, 1, 0) == 0)
        {
            _sweepTimer.Change(InterpreterCacheLifetime, Timeout.InfiniteTimeSpan);
        }
    }

    private bool IsExpired(PooledJavaScriptEngine rental, long now)
        => _timeProvider.GetElapsedTime(rental.InterpreterStateSince, now) >= InterpreterCacheLifetime;

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

    // Whether the engine's interpreter caches may hold objects of a finished evaluation, and since when (a
    // TimeProvider timestamp). Written only by whoever has the engine out of its slot — the pool while
    // returning it, or a sweep that took it out — and published by the interlocked exchange that puts it back.
    internal bool HoldsInterpreterState;

    internal long InterpreterStateSince;
}
