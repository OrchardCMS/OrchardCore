using Acornima.Ast;
using Jint;
using Jint.Native;
using Jint.Runtime.Descriptors;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using JintOptions = Jint.Options;

namespace OrchardCore.Scripting.JavaScript;

public sealed class JavaScriptEngine : IScriptingEngine
{
    private static readonly MemoryCacheEntryOptions ScriptCacheEntryOptions = new MemoryCacheEntryOptions()
        .SetSlidingExpiration(TimeSpan.FromMinutes(30))
        ;

    // The attributes Engine.SetValue(string, Delegate) gives a global, so that a lazily declared global is
    // indistinguishable from an eagerly set one once it is materialized.
    private const PropertyFlag GlobalPropertyFlags = PropertyFlag.NonEnumerable;

    private readonly IMemoryCache _memoryCache;
    private readonly JintOptions _jintOptions;
    private readonly Dictionary<string, LazyGlobalMethod> _lazyGlobals;

    public JavaScriptEngine(
        IMemoryCache memoryCache,
        IOptions<JintOptions> jintOptions,
        IEnumerable<IGlobalMethodProvider> globalMethodProviders)
    {
        _memoryCache = memoryCache;
        _jintOptions = jintOptions.Value;
        _jintOptions.ExperimentalFeatures |= ExperimentalFeature.TaskInterop;
        _lazyGlobals = RegisterLazyGlobals(_jintOptions, globalMethodProviders);
    }

    public string Prefix => "js";

    /// <summary>
    /// Creates a scope backed by a new engine.
    /// </summary>
    /// <remarks>
    /// The globals of the registered <see cref="IGlobalMethodProvider"/> instances are installed on every
    /// engine as lazy properties, whether or not <paramref name="methods"/> contains them. A lazy property
    /// only builds its delegate when a script actually reads the name, so the ones a script does not use
    /// cost nothing. Methods that are not registered by a provider, such as the ones a caller adds for a
    /// single evaluation, are installed eagerly and take precedence over a registered global of the same name.
    /// </remarks>
    public IScriptingScope CreateScope(IEnumerable<GlobalMethod> methods, IServiceProvider serviceProvider, IFileProvider fileProvider, string basePath)
    {
        var engine = new Engine(_jintOptions);

        return new JavaScriptScope(engine, serviceProvider, methods, _lazyGlobals);
    }

    public object Evaluate(IScriptingScope scope, string script)
    {
        var jsScope = GetJavaScriptScope(scope);

        var result = jsScope.Engine.Evaluate(PrepareScript(script)).ToObject();

        return result;
    }

    public async Task<object> EvaluateAsync(IScriptingScope scope, string script, CancellationToken cancellationToken = default)
    {
        var jsScope = GetJavaScriptScope(scope);

        var result = await jsScope.Engine.EvaluateAsync(PrepareScript(script), cancellationToken);

        return result.ToObject();
    }

    private Prepared<Script> PrepareScript(string script)
        => _memoryCache.GetOrCreate(
            new PreparedScriptCacheKey(script),
            static entry => Engine.PrepareScript(((PreparedScriptCacheKey)entry.Key).Script),
            ScriptCacheEntryOptions);

    private static JavaScriptScope GetJavaScriptScope(IScriptingScope scope)
    {
        if (scope is not JavaScriptScope jsScope)
        {
            throw new ArgumentException($"Expected a scope of type {nameof(JavaScriptScope)}", nameof(scope));
        }

        return jsScope;
    }

    /// <summary>
    /// Namespaces the prepared script entries stored in the shared <see cref="IMemoryCache"/>. Using a
    /// dedicated key type instead of the raw script text guarantees that a key registered by another
    /// component cannot be read back as a prepared script.
    /// </summary>
    private readonly record struct PreparedScriptCacheKey(string Script);

    /// <summary>
    /// Declares the globals of the registered method providers on the shared options, so that every engine
    /// built from them gets its own lazy property per global instead of a delegate that has to be created,
    /// reflected over and wrapped up front. The options replay the declarations for each engine, so a global
    /// is materialized at most once per engine, on the first read of its name.
    /// </summary>
    private static Dictionary<string, LazyGlobalMethod> RegisterLazyGlobals(JintOptions options, IEnumerable<IGlobalMethodProvider> globalMethodProviders)
    {
        var candidates = new Dictionary<string, GlobalMethod>(StringComparer.Ordinal);
        var ambiguous = new HashSet<string>(StringComparer.Ordinal);

        foreach (var method in globalMethodProviders.SelectMany(provider => provider.GetMethods()))
        {
            if (!candidates.TryAdd(method.Name, method))
            {
                // Several providers contribute a global with the same name. Which one wins depends on the
                // order they are set in, so leave them all to the eager path where that order is observable.
                ambiguous.Add(method.Name);
            }
        }

        var lazyGlobals = new Dictionary<string, LazyGlobalMethod>(candidates.Count, StringComparer.Ordinal);

        foreach (var (name, method) in candidates)
        {
            // A method named 'x' with an asynchronous variant and a method named 'xAsync' would both claim
            // the 'xAsync' global; keep those on the eager path as well.
            if (ambiguous.Contains(name) || (method.AsyncMethod != null && candidates.ContainsKey(name + "Async")))
            {
                continue;
            }

            var hasSyncGlobal = method.Method != null;
            var hasAsyncGlobal = method.AsyncMethod != null;

            if (hasSyncGlobal)
            {
                var factory = method.Method;
                options.AddLazyGlobal(name, engine => CreateGlobal(engine, name, factory), GlobalPropertyFlags);
            }

            if (hasAsyncGlobal)
            {
                var asyncName = name + "Async";
                var factory = method.AsyncMethod;
                options.AddLazyGlobal(asyncName, engine => CreateGlobal(engine, asyncName, factory), GlobalPropertyFlags);
            }

            if (hasSyncGlobal || hasAsyncGlobal)
            {
                lazyGlobals[name] = new LazyGlobalMethod(method, hasSyncGlobal, hasAsyncGlobal);
            }
        }

        return lazyGlobals;
    }

    private static JsValue CreateGlobal(Engine engine, string name, Func<IServiceProvider, Delegate> factory)
    {
        // The factory captures the services it is given, so the delegate has to be built with the services
        // of the scope that owns this engine, and cannot be shared between engines.
        if (!JavaScriptScope.TryGetServiceProvider(engine, out var serviceProvider))
        {
            // The lazy property stores whatever this returns and never runs again, so returning a value here
            // would leave the global permanently undefined and fail as 'x is not a function' somewhere else.
            // Reaching this means an engine was built from these options without a scope, which is a defect
            // in the caller rather than a state a script should have to cope with.
            throw new InvalidOperationException(
                $"No scripting scope is associated with the engine reading the global '{name}'. Engines that expose the globals of the registered {nameof(IGlobalMethodProvider)} instances must be created through {nameof(IScriptingEngine)}.{nameof(CreateScope)}.");
        }

        // This is only equivalent to what Engine.SetValue(string, Delegate) installs while no IObjectConverter
        // is registered on the options: FromObject consults the registered converters before it falls back to
        // wrapping the delegate, so a converter handling Delegate would give a materialized global a different
        // shape than an eagerly set one, in the same engine.
        return JsValue.FromObject(engine, factory(serviceProvider));
    }

    /// <summary>
    /// A <see cref="GlobalMethod"/> whose globals are declared on the engine options, and which of the two
    /// globals it can contribute are covered by that declaration.
    /// </summary>
    internal readonly record struct LazyGlobalMethod(GlobalMethod Method, bool HasSyncGlobal, bool HasAsyncGlobal);
}
