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

        public TestHost(int? poolSize = null, Action<Jint.Options> configureJint = null)
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
