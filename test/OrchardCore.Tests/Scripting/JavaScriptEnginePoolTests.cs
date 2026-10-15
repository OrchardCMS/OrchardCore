using System.Runtime.CompilerServices;
using Jint;
using Jint.Runtime;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;

namespace OrchardCore.Tests.Scripting;

/// <summary>
/// Pins the engine reuse that <see cref="JavaScriptEngine"/> does between evaluations: what a reused engine
/// must have forgotten, and what must be true of it even when a caller uses a scope badly.
/// </summary>
public class JavaScriptEnginePoolTests
{
    [Fact]
    public void DisposingAScope_LetsTheNextScopeReuseTheEngine()
    {
        var host = new TestHost();

        var first = host.CreateScope();
        var engine = first.Engine;
        first.Dispose();

        using var second = host.CreateScope();

        Assert.Same(engine, second.Engine);
    }

    [Fact]
    public void GlobalsDeclaredByOneEvaluation_AreNotVisibleToTheNext()
    {
        var host = new TestHost();

        var first = host.CreateScope();
        var engine = first.Engine;

        Assert.Equal(1, Convert.ToInt32(host.Evaluate(first, "globalThis.leaked = 1; var alsoLeaked = 2; return 1;")));

        first.Dispose();

        using var second = host.CreateScope();

        Assert.Same(engine, second.Engine);
        Assert.Equal("undefined,undefined", host.Evaluate(second, "return typeof leaked + ',' + typeof alsoLeaked;"));
    }

    [Fact]
    public void LexicalDeclarationsMadeByOneEvaluation_AreNotVisibleToTheNext()
    {
        var host = new TestHost();

        var first = host.CreateScope();
        var engine = first.Engine;

        // Nothing in Jint's public API other than a snapshot restore can undo a top-level let/const, so
        // re-running the same script on the same engine would otherwise fail with a redeclaration error.
        Assert.Equal(1, Convert.ToInt32(host.Evaluate(first, "let declaredOnce = 1; return declaredOnce;")));

        first.Dispose();

        using var second = host.CreateScope();

        Assert.Same(engine, second.Engine);
        Assert.Equal(2, Convert.ToInt32(host.Evaluate(second, "let declaredOnce = 2; return declaredOnce;")));
    }

    [Fact]
    public void AScriptThatThrows_StillLeavesACleanEngineForTheNextScope()
    {
        var host = new TestHost();

        var first = host.CreateScope();
        var engine = first.Engine;

        try
        {
            Assert.Throws<JavaScriptException>(() => host.Evaluate(first, "globalThis.dirty = 1; throw new Error('boom');"));
        }
        finally
        {
            first.Dispose();
        }

        using var second = host.CreateScope();

        Assert.Same(engine, second.Engine);
        Assert.Equal("undefined", host.Evaluate(second, "return typeof dirty;"));
    }

    [Fact]
    public void ARegisteredGlobal_IsRebuiltFromTheServicesOfTheScopeThatReadsIt()
    {
        // The whole reason a reused engine is safe: a registered global is a delegate built from the
        // services of one evaluation, so reusing the engine without putting the global back in its
        // not-yet-built state would serve the next request a delegate closed over the previous request's
        // service provider.
        var host = new TestHost();

        using var firstServices = host.CreateServiceScope("first");
        var first = host.CreateScope(firstServices);
        var engine = first.Engine;

        Assert.Equal("first", host.Evaluate(first, "return owningScope();"));
        Assert.Equal(1, host.Provider.BuildCount);

        first.Dispose();

        using var secondServices = host.CreateServiceScope("second");
        using var second = host.CreateScope(secondServices);

        Assert.Same(engine, second.Engine);
        Assert.Equal("second", host.Evaluate(second, "return owningScope();"));
        Assert.Equal(2, host.Provider.BuildCount);
    }

    [Fact]
    public void AMethodShadowingARegisteredGlobal_DoesNotOutliveItsScope()
    {
        var host = new TestHost();

        var shadow = new GlobalMethod
        {
            Name = "owningScope",
            Method = _ => (Func<string>)(() => "shadowed"),
        };

        using var firstServices = host.CreateServiceScope("first");
        var first = host.CreateScope(firstServices, shadow);
        var engine = first.Engine;

        Assert.Equal("shadowed", host.Evaluate(first, "return owningScope();"));

        first.Dispose();

        using var secondServices = host.CreateServiceScope("second");
        using var second = host.CreateScope(secondServices);

        Assert.Same(engine, second.Engine);
        Assert.Equal("second", host.Evaluate(second, "return owningScope();"));
    }

    [Fact]
    public async Task APromiseRegisteredBeforeAResetDoesNotSettleIntoTheNextEvaluation()
    {
        var host = new TestHost();

        using var firstServices = host.CreateServiceScope("first");
        var first = host.CreateScope(firstServices);
        var engine = first.Engine;

        // A fire-and-forget async function suspended on a CLR task. Nothing awaits the promise it returns,
        // so the assignment happens whenever the task completes — which here is after the scope has ended.
        host.Evaluate(first, "(async () => { globalThis.settled = await pendingAsync(); })(); return 1;");

        first.Dispose();

        host.Pending.SetResult("late");

        // Give the task's continuation a chance to reach the engine. The assertion holds whether or not it
        // gets there in time, so this cannot make the test flaky; it only makes it exercise the fence.
        await Task.Delay(100, TestContext.Current.CancellationToken);

        using var secondServices = host.CreateServiceScope("second");
        using var second = host.CreateScope(secondServices);

        Assert.Same(engine, second.Engine);
        Assert.Equal("undefined", host.Evaluate(second, "return typeof settled;"));
    }

    [Fact]
    public async Task ConcurrentScopes_NeverShareAnEngine()
    {
        var host = new TestHost(poolSize: 4);

        var inUse = new ConcurrentDictionary<Engine, Holder>();
        var shared = 0;

        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
        {
            for (var iteration = 0; iteration < 40; iteration++)
            {
                var scope = host.CreateScope();
                var holder = inUse.GetOrAdd(scope.Engine, static _ => new Holder());

                if (Interlocked.Increment(ref holder.Count) != 1)
                {
                    Interlocked.Increment(ref shared);
                }

                try
                {
                    // Every scope starts from a reset engine, so the counter can only ever reach 1. Two
                    // scopes sharing an engine would be caught here as well as by the holder above.
                    var marks = Convert.ToInt32(host.Evaluate(scope, "globalThis.marks = (globalThis.marks || 0) + 1; return globalThis.marks;"));

                    Assert.Equal(1, marks);
                }
                finally
                {
                    // Released before the engine is, because disposing is what makes it available to
                    // another worker.
                    Interlocked.Decrement(ref holder.Count);
                    scope.Dispose();
                }
            }
        })));

        Assert.Equal(0, shared);
    }

    [Fact]
    public void ALeakedScope_KeepsItsEngineOutOfThePoolWithoutDisturbingIt()
    {
        var host = new TestHost();

        // Deliberately never disposed: the failure mode of a caller that forgets has to be "this engine is
        // never reused", never "this engine is handed to somebody else as well".
        var leaked = host.CreateScope();
        var leakedEngine = leaked.Engine;

        var second = host.CreateScope();
        var secondEngine = second.Engine;

        Assert.NotSame(leakedEngine, secondEngine);

        second.Dispose();

        using var third = host.CreateScope();

        Assert.Same(secondEngine, third.Engine);

        // The leaked scope keeps working; it simply owns its engine forever.
        Assert.Equal(2, Convert.ToInt32(host.Evaluate(leaked, "return 1 + 1;")));
    }

    [Fact]
    public void DisposingAScopeTwice_ReleasesTheEngineOnce()
    {
        var host = new TestHost();

        var scope = host.CreateScope();
        var engine = scope.Engine;

        scope.Dispose();
        scope.Dispose();

        using var second = host.CreateScope();
        using var third = host.CreateScope();

        // Had the double disposal put the engine into two slots, both of these would be the same instance
        // and two callers would be evaluating on one engine.
        Assert.Same(engine, second.Engine);
        Assert.NotSame(second.Engine, third.Engine);
    }

    [Fact]
    public void UsingAScopeAfterItHasBeenDisposed_Throws()
    {
        var host = new TestHost();

        var scope = host.CreateScope();
        scope.Dispose();

        Assert.Throws<ObjectDisposedException>(() => host.Evaluate(scope, "return 1;"));
    }

    [Fact]
    public void ExecutionConstraints_AreRewoundForEachEvaluation()
    {
        // A tenant configures its statement budget and its timeout once, on options shared by every engine.
        // Both keep per-execution state, so a reused engine that did not rewind them would start each
        // evaluation with the previous one's budget already spent.
        var host = new TestHost(configureJint: options => options.MaxStatements(50));

        var first = host.CreateScope();
        var engine = first.Engine;

        Assert.Throws<StatementsCountOverflowException>(() => host.Evaluate(first, "for (var i = 0; i < 1000; i++) { }"));

        first.Dispose();

        using var second = host.CreateScope();

        Assert.Same(engine, second.Engine);
        Assert.Equal(1, Convert.ToInt32(host.Evaluate(second, "return 1;")));
    }

    [Fact]
    public void APoolSizeOfZero_BuildsANewEngineForEveryScope()
    {
        var host = new TestHost(poolSize: 0);

        var first = host.CreateScope();
        var engine = first.Engine;
        first.Dispose();

        using var second = host.CreateScope();

        Assert.NotSame(engine, second.Engine);
    }

    [Fact]
    public void ThePoolSize_BoundsHowManyEnginesAreKept()
    {
        var host = new TestHost(poolSize: 2);

        var scopes = new[] { host.CreateScope(), host.CreateScope(), host.CreateScope() };
        var firstRound = scopes.Select(scope => scope.Engine).ToArray();

        Assert.Equal(3, firstRound.Distinct().Count());

        foreach (var scope in scopes)
        {
            scope.Dispose();
        }

        var reused = new[] { host.CreateScope(), host.CreateScope(), host.CreateScope() };
        var secondRound = reused.Select(scope => scope.Engine).ToArray();

        foreach (var scope in reused)
        {
            scope.Dispose();
        }

        // Two of the three were kept; the third was dropped when the pool had no free slot for it.
        Assert.Equal(2, secondRound.Intersect(firstRound).Count());
        Assert.Equal(4, firstRound.Concat(secondRound).Distinct().Count());
    }

    [Fact]
    public void AReturnedEngine_NoLongerCarriesTheServicesOfTheScopeThatUsedIt()
    {
        // The services of an evaluation travel on the engine, in its HostDefined slot, which a global
        // snapshot does not cover. An idle engine that still carried them would keep a finished request's
        // services alive for as long as the engine sits in the pool.
        var host = new TestHost();

        using var services = host.CreateServiceScope("first");
        var scope = host.CreateScope(services);
        var engine = scope.Engine;

        Assert.Same(services.ServiceProvider, engine.Advanced.HostDefined);
        Assert.Equal("first", host.Evaluate(scope, "return owningScope();"));

        scope.Dispose();

        Assert.Null(engine.Advanced.HostDefined);
    }

    [Fact]
    public void AnEngineReachedAfterItsScopeEnded_CannotBuildAGlobalFromAFinishedScope()
    {
        // A caller that kept the engine past the end of its scope - by holding on to it, or to a function a
        // script returned - must fail on the first registered global it reads, rather than quietly build
        // that global from the services of an evaluation that is over.
        var host = new TestHost();

        using var services = host.CreateServiceScope("first");
        var scope = host.CreateScope(services);
        var engine = scope.Engine;

        scope.Dispose();

        Assert.Throws<InvalidOperationException>(() => engine.Evaluate("owningScope()"));
        Assert.Equal(0, host.Provider.BuildCount);
    }

    [Fact]
    public async Task ACancelledEvaluation_LeavesACleanEngineThatTheNextEvaluationCanUse()
    {
        // Cancelling stops the interpreter by throwing out of the middle of the script, so whatever it did
        // before that point is left on the engine, and the evaluation's token is armed on the engine's
        // deadline constraint. A reused engine must have shed both.
        var host = new TestHost();

        var first = host.CreateScope();
        var engine = first.Engine;

        using (var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken))
        {
            cancellation.CancelAfter(TimeSpan.FromMilliseconds(50));

            try
            {
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    () => host.EvaluateAsync(first, "globalThis.dirty = 1; while (true) { }", cancellation.Token));
            }
            finally
            {
                first.Dispose();
            }
        }

        using var second = host.CreateScope();

        Assert.Same(engine, second.Engine);
        Assert.Equal("undefined", host.Evaluate(second, "return typeof dirty;"));

        // The cancelled token must not have stayed armed: a loop long enough to reach the constraint's
        // amortized check several times over runs to completion, synchronously and asynchronously.
        Assert.Equal(10_000, Convert.ToInt32(host.Evaluate(second, "var n = 0; for (var i = 0; i < 10000; i++) { n++; } return n;")));
        Assert.Equal(10_000, Convert.ToInt32(await host.EvaluateAsync(second, "let m = 0; for (let i = 0; i < 10000; i++) { m++; } return m;", TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task AnEvaluationCancelledWhileAwaiting_DoesNotSettleIntoTheNextEvaluation()
    {
        // Cancelled while the script waits on a CLR task rather than while it runs: the continuation is still
        // registered when the scope ends, and the task completes afterwards.
        var host = new TestHost();

        var first = host.CreateScope();

        using (var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken))
        {
            cancellation.CancelAfter(TimeSpan.FromMilliseconds(50));

            try
            {
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    () => host.EvaluateAsync(first, "return (async () => { globalThis.settled = await pendingAsync(); })();", cancellation.Token));
            }
            finally
            {
                first.Dispose();
            }
        }

        host.Pending.SetResult("late");

        // As in the uncancelled case above, the delay only gives the continuation a chance to arrive; the
        // assertion holds either way.
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Whether the engine was reset and kept or dropped as unresettable is the pool's business; either
        // way the next evaluation must neither see the late result nor fail.
        using var second = host.CreateScope();

        Assert.Equal("undefined", host.Evaluate(second, "return typeof settled;"));
        Assert.Equal(3, Convert.ToInt32(await host.EvaluateAsync(second, "return 1 + 2;", TestContext.Current.CancellationToken)));
    }

    [Fact]
    public void ATimedOutEvaluation_LeavesACleanEngineWithAFreshDeadline()
    {
        // A site's TimeoutInterval is a deadline armed per evaluation. One that ran out must not leave the
        // next evaluation on the same engine starting with the time already spent.
        var host = new TestHost(configureJint: options => options.TimeoutInterval(TimeSpan.FromMilliseconds(100)));

        var first = host.CreateScope();
        var engine = first.Engine;

        try
        {
            Assert.Throws<TimeoutException>(() => host.Evaluate(first, "globalThis.dirty = 1; while (true) { }"));
        }
        finally
        {
            first.Dispose();
        }

        using var second = host.CreateScope();

        Assert.Same(engine, second.Engine);
        Assert.Equal("undefined", host.Evaluate(second, "return typeof dirty;"));
    }

    [Fact]
    public void AnUnboundedRecursion_LeavesACleanEngineForTheNextScope()
    {
        // The stack guard turns the overflow into a RangeError thrown from deep inside the recursion, so the
        // engine is unwound from hundreds of frames before it is reset and reused.
        var host = new TestHost();

        var first = host.CreateScope();
        var engine = first.Engine;

        try
        {
            Assert.Throws<JavaScriptException>(() => host.Evaluate(first, "globalThis.dirty = 1; function f() { return 1 + f(); } return f();"));
        }
        finally
        {
            first.Dispose();
        }

        using var second = host.CreateScope();

        Assert.Same(engine, second.Engine);
        Assert.Equal("undefined,undefined", host.Evaluate(second, "return typeof dirty + ',' + typeof f;"));
        Assert.Equal(100, Convert.ToInt32(host.Evaluate(second, "function g(n) { return n === 0 ? 0 : 1 + g(n - 1); } return g(100);")));
    }

    [Fact]
    public void AnIdleEngine_LetsGoOfAFinishedRequestsServices_OnceItsInterpreterStateExpires()
    {
        var time = new ManualTimeProvider();
        var host = new TestHost(timeProvider: time);

        var services = RunTwiceOnOneEngine(host, out var engine);

        // The control: the engine is idle in the pool, its global surface reset and its HostDefined slot
        // cleared, and yet the call site that ran warm still reaches the finished request's services.
        CollectGarbage();
        Assert.True(services.IsAlive);
        Assert.True(time.HasScheduledTimer);

        // Nothing is evaluated after this; only the sweep can release them.
        time.Advance(JavaScriptEnginePool.InterpreterCacheLifetime);
        time.RunDueTimers();

        CollectGarbage();
        Assert.False(services.IsAlive);

        // A tenant that has stopped evaluating scripts has no timer running for it.
        Assert.False(time.HasScheduledTimer);

        // The engine is still pooled and still works, and builds its registered globals from the next scope.
        using var nextServices = host.CreateServiceScope("next");
        using var next = host.CreateScope(nextServices);

        Assert.Same(engine, next.Engine);
        Assert.Equal("next", host.Evaluate(next, "return owningScope();"));
    }

    [Fact]
    public void AnEngineInUseWhenItsInterpreterStateExpires_IsLeftAloneUntilItIsReturned()
    {
        var time = new ManualTimeProvider();
        var host = new TestHost(timeProvider: time);

        var services = RunTwiceOnOneEngine(host, out var engine);

        using var holdingServices = host.CreateServiceScope("holding");
        var holding = host.CreateScope(holdingServices);

        Assert.Same(engine, holding.Engine);

        // The sweep runs while the engine is rented. Had it discarded the engine's state underneath the
        // scope, the finished request's services would be collectable now.
        time.Advance(JavaScriptEnginePool.InterpreterCacheLifetime);
        time.RunDueTimers();

        CollectGarbage();
        Assert.True(services.IsAlive);

        // Returning an engine whose state has expired discards it there, so an engine that is always busy
        // when a sweep looks does not keep a finished request's objects either.
        holding.Dispose();

        CollectGarbage();
        Assert.False(services.IsAlive);
        Assert.False(time.HasScheduledTimer);
    }

    [Fact]
    public async Task ConcurrentScopes_AreNotDisturbedBySweeps()
    {
        var time = new ManualTimeProvider();
        var host = new TestHost(poolSize: 4, timeProvider: time);

        using var stop = new CancellationTokenSource();

        // Every engine is always past its lifetime, so each return discards and every sweep that finds an
        // idle engine takes it out of its slot and discards it too.
        var sweeper = Task.Run(() =>
        {
            while (!stop.IsCancellationRequested)
            {
                time.Advance(JavaScriptEnginePool.InterpreterCacheLifetime);
                time.RunAllTimers();
            }
        }, TestContext.Current.CancellationToken);

        try
        {
            await Task.WhenAll(Enumerable.Range(0, 8).Select(worker => Task.Run(() =>
            {
                for (var iteration = 0; iteration < 40; iteration++)
                {
                    var name = $"{worker}-{iteration}";

                    using var services = host.CreateServiceScope(name);
                    using var scope = host.CreateScope(services);

                    Assert.Equal(name, host.Evaluate(scope, "function read() { return owningScope(); } read(); return read();"));
                }
            }, TestContext.Current.CancellationToken)));
        }
        finally
        {
            await stop.CancelAsync();
            await sweeper;
        }
    }

    /// <summary>
    /// Runs the same script on the same engine for two requests, so that its call site is warm, and returns
    /// a weak reference to the second request's service provider. Not inlined, so that nothing on the
    /// caller's stack keeps the provider alive.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference RunTwiceOnOneEngine(TestHost host, out Engine engine)
    {
        engine = null;
        IServiceProvider lastServices = null;

        foreach (var name in new[] { "first", "second" })
        {
            using var services = host.CreateServiceScope(name);
            using var scope = host.CreateScope(services);

            engine ??= scope.Engine;

            Assert.Same(engine, scope.Engine);
            Assert.Equal(name, host.Evaluate(scope, "return owningScope();"));

            lastServices = services.ServiceProvider;
        }

        return new WeakReference(lastServices);
    }

    private static void CollectGarbage()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private sealed class Holder
    {
        public int Count;
    }

    /// <summary>
    /// A tenant's worth of scripting services, with one registered global whose value identifies the service
    /// scope it was built from and one that suspends on a task the test controls.
    /// </summary>
    private sealed class TestHost
    {
        private readonly IServiceProvider _rootServices;
        private readonly IScriptingEngine _engine;
        private readonly GlobalMethod[] _methods;

        public TestHost(int? poolSize = null, Action<Jint.Options> configureJint = null, TimeProvider timeProvider = null)
        {
            Provider = new OwningScopeMethodProvider(this);

            var services = new ServiceCollection()
                .AddMemoryCache()
                .AddScripting()
                .AddJavaScriptEngine()
                .AddScoped<ServiceScopeName>()
                .AddSingleton<IGlobalMethodProvider>(Provider);

            if (poolSize.HasValue)
            {
                services.Configure<JavaScriptEngineOptions>(options => options.EnginePoolSize = poolSize.Value);
            }

            if (configureJint != null)
            {
                services.Configure(configureJint);
            }

            if (timeProvider != null)
            {
                services.Configure<JavaScriptEngineOptions>(options => options.TimeProvider = timeProvider);
            }

            _rootServices = services.BuildServiceProvider();

            var scriptingManager = _rootServices.GetRequiredService<IScriptingManager>();

            _engine = scriptingManager.GetScriptingEngine("js");
            _methods = scriptingManager.GlobalMethodProviders.SelectMany(provider => provider.GetMethods()).ToArray();
        }

        public OwningScopeMethodProvider Provider { get; }

        public TaskCompletionSource<string> Pending { get; } = new();

        public IServiceScope CreateServiceScope(string name)
        {
            var serviceScope = _rootServices.CreateScope();
            serviceScope.ServiceProvider.GetRequiredService<ServiceScopeName>().Value = name;

            return serviceScope;
        }

        public JavaScriptScope CreateScope()
            => (JavaScriptScope)_engine.CreateScope(_methods, _rootServices, null, null);

        public JavaScriptScope CreateScope(IServiceScope serviceScope, params GlobalMethod[] extraMethods)
            => (JavaScriptScope)_engine.CreateScope(_methods.Concat(extraMethods), serviceScope.ServiceProvider, null, null);

        public object Evaluate(IScriptingScope scope, string script)
            => _engine.Evaluate(scope, script);

        public Task<object> EvaluateAsync(IScriptingScope scope, string script, CancellationToken cancellationToken)
            => _engine.EvaluateAsync(scope, script, cancellationToken);
    }

    /// <summary>
    /// A clock that only moves when the test moves it, and timers that only fire when the test runs them.
    /// </summary>
    private sealed class ManualTimeProvider : TimeProvider
    {
        private readonly List<ManualTimer> _timers = [];
        private long _now;

        public override long TimestampFrequency => TimeSpan.TicksPerSecond;

        public override long GetTimestamp() => Interlocked.Read(ref _now);

        public override DateTimeOffset GetUtcNow() => new(Interlocked.Read(ref _now), TimeSpan.Zero);

        public void Advance(TimeSpan by) => Interlocked.Add(ref _now, by.Ticks);

        public bool HasScheduledTimer => Timers().Any(timer => timer.DueAt.HasValue);

        public override ITimer CreateTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            var timer = new ManualTimer(this, callback, state);
            timer.Change(dueTime, period);

            lock (_timers)
            {
                _timers.Add(timer);
            }

            return timer;
        }

        /// <summary>
        /// Runs the timers that are due, as the system clock would have by now.
        /// </summary>
        public void RunDueTimers()
        {
            foreach (var timer in Timers())
            {
                timer.RunIfDue(GetTimestamp());
            }
        }

        /// <summary>
        /// Runs every timer whether it is scheduled or not, which a sweep has to tolerate at any moment.
        /// </summary>
        public void RunAllTimers()
        {
            foreach (var timer in Timers())
            {
                timer.Run();
            }
        }

        private ManualTimer[] Timers()
        {
            lock (_timers)
            {
                return [.. _timers];
            }
        }

        private sealed class ManualTimer(ManualTimeProvider time, TimerCallback callback, object state) : ITimer
        {
            private readonly Lock _lock = new();
            private long? _dueAt;

            public long? DueAt
            {
                get
                {
                    lock (_lock)
                    {
                        return _dueAt;
                    }
                }
            }

            public bool Change(TimeSpan dueTime, TimeSpan period)
            {
                lock (_lock)
                {
                    _dueAt = dueTime == Timeout.InfiniteTimeSpan ? null : time.GetTimestamp() + dueTime.Ticks;
                }

                return true;
            }

            public void RunIfDue(long now)
            {
                lock (_lock)
                {
                    if (_dueAt is not { } dueAt || dueAt > now)
                    {
                        return;
                    }

                    _dueAt = null;
                }

                callback(state);
            }

            public void Run()
            {
                lock (_lock)
                {
                    _dueAt = null;
                }

                callback(state);
            }

            public void Dispose() => Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            public ValueTask DisposeAsync()
            {
                Dispose();

                return ValueTask.CompletedTask;
            }
        }
    }

    private sealed class ServiceScopeName
    {
        public string Value { get; set; }
    }

    private sealed class OwningScopeMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod[] _methods;

        public OwningScopeMethodProvider(TestHost host)
        {
            _methods =
            [
                new GlobalMethod
                {
                    Name = "owningScope",
                    Method = serviceProvider =>
                    {
                        BuildCount++;

                        return (Func<string>)(() => serviceProvider.GetRequiredService<ServiceScopeName>().Value);
                    },
                },
                new GlobalMethod
                {
                    Name = "pending",
                    AsyncMethod = _ => (Func<Task<string>>)(() => host.Pending.Task),
                },
            ];
        }

        public int BuildCount { get; private set; }

        public IEnumerable<GlobalMethod> GetMethods() => _methods;
    }
}
