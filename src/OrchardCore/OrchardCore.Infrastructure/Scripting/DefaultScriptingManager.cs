using System.Collections.Frozen;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Scripting;

public class DefaultScriptingManager : IScriptingManager
{
    private readonly FrozenDictionary<string, IScriptingEngine> _enginesByPrefix;
    private readonly GlobalMethod[] _globalMethods;

    public DefaultScriptingManager(
        IEnumerable<IScriptingEngine> engines,
        IEnumerable<IGlobalMethodProvider> globalMethodProviders)
    {
        // Resolving the engine used to walk the registrations on every evaluation and keep the first one
        // whose prefix matched. Two engines can in principle claim the same prefix, so the index is built
        // with TryAdd rather than an indexer assignment: an engine registered later must not be able to
        // displace one that already answered for that prefix.
        var enginesByPrefix = new Dictionary<string, IScriptingEngine>(StringComparer.Ordinal);

        foreach (var engine in engines)
        {
            // A directive always carries a prefix, so an engine that declares none can never be addressed
            // by one and has nothing to be indexed under.
            if (engine.Prefix is not null)
            {
                enginesByPrefix.TryAdd(engine.Prefix, engine);
            }
        }

        _enginesByPrefix = enginesByPrefix.ToFrozenDictionary(StringComparer.Ordinal);

        GlobalMethodProviders = new List<IGlobalMethodProvider>(globalMethodProviders).AsReadOnly();

        // Every evaluation builds a scope out of the same registered methods, and an evaluation is cheap
        // enough that the enumerators of a per-evaluation LINQ pipeline are a visible share of it. The
        // providers are singletons of the tenant and contribute a fixed set of methods once they are
        // resolved, so the flattening is done once here instead.
        _globalMethods = GlobalMethodProviders.SelectMany(provider => provider.GetMethods()).ToArray();
    }

    public IReadOnlyList<IGlobalMethodProvider> GlobalMethodProviders { get; }

    public object Evaluate(string directive,
        IFileProvider fileProvider,
        string basePath,
        IEnumerable<IGlobalMethodProvider> scopedMethodProviders)
    {
        if (!TryParseDirective(directive, out var prefix, out var script))
        {
            return directive;
        }

        var engine = GetScriptingEngine(prefix);
        if (engine == null)
        {
            return directive;
        }

        var scope = engine.CreateScope(GetMethods(scopedMethodProviders), ShellScope.Services, fileProvider, basePath);

        return engine.Evaluate(scope, script);
    }

    public async Task<object> EvaluateAsync(string directive,
        IFileProvider fileProvider,
        string basePath,
        IEnumerable<IGlobalMethodProvider> scopedMethodProviders,
        CancellationToken cancellationToken = default)
    {
        if (!TryParseDirective(directive, out var prefix, out var script))
        {
            return directive;
        }

        var engine = GetScriptingEngine(prefix);
        if (engine == null)
        {
            return directive;
        }

        var scope = engine.CreateScope(GetMethods(scopedMethodProviders), ShellScope.Services, fileProvider, basePath);

        return await engine.EvaluateAsync(scope, script, cancellationToken);
    }

    public IScriptingEngine GetScriptingEngine(string prefix)
    {
        // A prefix parsed out of a directive is never null, but this is public surface and the scan this
        // replaces accepted a null one and found nothing for it, which is the one question the index
        // cannot be asked: no engine is indexed under a prefix it cannot declare.
        if (prefix is null)
        {
            return null;
        }

        _enginesByPrefix.TryGetValue(prefix, out var engine);

        return engine;
    }

    private IEnumerable<GlobalMethod> GetMethods(IEnumerable<IGlobalMethodProvider> scopedMethodProviders)
    {
        if (scopedMethodProviders is null)
        {
            return _globalMethods;
        }

        // The scoped providers belong to this evaluation alone, so their methods are read every time. They
        // come last because a scope installs the methods in order, which is what lets a scoped method
        // shadow a registered one of the same name.
        return _globalMethods.Concat(scopedMethodProviders.SelectMany(static provider => provider.GetMethods()));
    }

    private static bool TryParseDirective(string directive, out string prefix, out string script)
    {
        var directiveIndex = directive.IndexOf(':');
        if (directiveIndex == -1 || directiveIndex >= directive.Length - 1)
        {
            prefix = null;
            script = null;
            return false;
        }

        prefix = directive[..directiveIndex];
        script = directive[(directiveIndex + 1)..];

        return true;
    }
}
