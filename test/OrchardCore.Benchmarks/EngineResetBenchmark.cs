using System;
using System.Collections.Generic;
using System.Linq;
using Acornima.Ast;
using BenchmarkDotNet.Attributes;
using Jint;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;
using JintOptions = Jint.Options;

namespace OrchardCore.Benchmarks;

/// <summary>
/// Weighs the two ways of giving an evaluation a clean set of globals: building an engine, which is what
/// every evaluated expression used to do, and resetting one that already exists, which is what reusing
/// engines does instead.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="GlobalCount"/> parameter is the axis that matters. A tenant registers on the order of
/// forty globals through <see cref="IGlobalMethodProvider"/>, and both operations scale with that number —
/// construction because it installs a lazy property per global, the reset because it rebuilds the global
/// object's property table. Running at zero as well as at forty separates the fixed cost of each operation
/// from its per-global slope, which is what decides whether reuse is worth its plumbing: if resetting costs
/// a large fraction of constructing, reuse buys only the engine's warm interpreter caches.
/// </para>
/// <para>
/// Each row that mutates an engine owns one, built in <see cref="GlobalSetup"/> and warmed with that row's
/// own script and nothing else. Sharing one engine between rows would make each row's number depend on which
/// other rows exist, because an engine caches interpreter state per script it has run and because the
/// scripts declare their globals on one global object.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class EngineResetBenchmark
{
    // The expression the campaign measured end to end: a literal, so that nothing but the cost of getting an
    // engine ready to evaluate on ends up in the number.
    private const string LiteralSource = "'literal'";

    // A registered global being read, which is the shape of a real directive such as [js:uuid()]. Only this
    // row makes the reset put a global that was actually built back into its unbuilt state.
    private const string GlobalSource = "global0()";

    private IServiceProvider _serviceProvider;
    private JintOptions _options;

    private Prepared<Script> _literal;
    private Prepared<Script> _global;

    private Engine _resetOnly;
    private GlobalSnapshot _resetOnlySnapshot;

    private Engine _evaluateLiteral;
    private GlobalSnapshot _evaluateLiteralSnapshot;

    private Engine _evaluateGlobal;
    private GlobalSnapshot _evaluateGlobalSnapshot;

    [Params(0, 40)]
    public int GlobalCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine();

        if (GlobalCount > 0)
        {
            services.AddSingleton<IGlobalMethodProvider>(new CountedMethodProvider(GlobalCount));
        }

        _serviceProvider = services.BuildServiceProvider();

        // Resolving the scripting engine is what declares the registered globals on the shared Jint options,
        // so the options can only be read back afterwards.
        _serviceProvider.GetRequiredService<IScriptingManager>().GetScriptingEngine("js");
        _options = _serviceProvider.GetRequiredService<IOptions<JintOptions>>().Value;

        _literal = Engine.PrepareScript(LiteralSource);
        _global = Engine.PrepareScript(GlobalCount > 0 ? GlobalSource : LiteralSource);

        _resetOnly = CreateWarmEngine(default, warmUp: false, out _resetOnlySnapshot);
        _evaluateLiteral = CreateWarmEngine(_literal, warmUp: true, out _evaluateLiteralSnapshot);
        _evaluateGlobal = CreateWarmEngine(_global, warmUp: true, out _evaluateGlobalSnapshot);
    }

    /// <summary>
    /// What <c>CreateScope</c> paid for every evaluated expression before engines were reused: an engine, its
    /// realm, and one lazy property per registered global.
    /// </summary>
    [Benchmark(Baseline = true)]
    public Engine NewEngine() => new(_options);

    /// <summary>
    /// What ending a scope pays instead. Against <see cref="NewEngine"/> this is the whole question.
    /// </summary>
    [Benchmark]
    public void ResetEngine() => _resetOnly.Advanced.RestoreGlobalSnapshot(_resetOnlySnapshot);

    [Benchmark]
    public object NewEngineThenEvaluateLiteral() => new Engine(_options).Evaluate(_literal).ToObject();

    [Benchmark]
    public object ResetEngineThenEvaluateLiteral()
    {
        var result = _evaluateLiteral.Evaluate(_literal).ToObject();
        _evaluateLiteral.Advanced.RestoreGlobalSnapshot(_evaluateLiteralSnapshot);

        return result;
    }

    /// <summary>
    /// The same pair over a script that reads a registered global, so that the reset has a global which was
    /// really built to put back into its unbuilt state, and the construction has one to build.
    /// </summary>
    [Benchmark]
    public object NewEngineThenEvaluateGlobal()
    {
        var engine = new Engine(_options);

        // Associating the engine with the services its registered globals are built from is part of what
        // creating a scope does, so it belongs in the row that stands for creating one.
        _ = new JavaScriptScope(engine, _serviceProvider, []);

        return engine.Evaluate(_global).ToObject();
    }

    [Benchmark]
    public object ResetEngineThenEvaluateGlobal()
    {
        var result = _evaluateGlobal.Evaluate(_global).ToObject();
        _evaluateGlobal.Advanced.RestoreGlobalSnapshot(_evaluateGlobalSnapshot);

        return result;
    }

    private Engine CreateWarmEngine(Prepared<Script> warmUpWith, bool warmUp, out GlobalSnapshot snapshot)
    {
        var engine = new Engine(_options);

        // A scope is what associates the engine with the services a registered global is built from; without
        // it, reading one throws.
        _ = new JavaScriptScope(engine, _serviceProvider, []);

        snapshot = engine.Advanced.CaptureGlobalSnapshot();

        if (warmUp)
        {
            // One run of this row's own script, so the row measures a warm engine the way a reused one is,
            // rather than the interpreter state Jint builds on a script's first evaluation.
            engine.Evaluate(warmUpWith);
            engine.Advanced.RestoreGlobalSnapshot(snapshot);
        }

        return engine;
    }

    private sealed class CountedMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod[] _methods;

        public CountedMethodProvider(int count)
        {
            _methods = Enumerable.Range(0, count)
                .Select(i => new GlobalMethod
                {
                    Name = "global" + i.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    Method = _ => (Func<string>)(() => "value"),
                })
                .ToArray();
        }

        public IEnumerable<GlobalMethod> GetMethods() => _methods;
    }
}
