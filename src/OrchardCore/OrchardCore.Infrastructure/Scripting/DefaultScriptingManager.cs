using Microsoft.Extensions.FileProviders;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Scripting;

public class DefaultScriptingManager : IScriptingManager
{
    private readonly IEnumerable<IScriptingEngine> _engines;

    public DefaultScriptingManager(
        IEnumerable<IScriptingEngine> engines,
        IEnumerable<IGlobalMethodProvider> globalMethodProviders)
    {
        _engines = engines;
        GlobalMethodProviders = new List<IGlobalMethodProvider>(globalMethodProviders).AsReadOnly();
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

        var methodProviders = scopedMethodProviders != null ? GlobalMethodProviders.Concat(scopedMethodProviders) : GlobalMethodProviders;
        var scope = engine.CreateScope(methodProviders.SelectMany(x => x.GetMethods()), ShellScope.Services, fileProvider, basePath);

        try
        {
            return engine.Evaluate(scope, script);
        }
        finally
        {
            // A scope may own state an engine wants back — the JavaScript engine reuses its engines between
            // evaluations and only learns the evaluation is over from here. The 'finally' is the point: a
            // script that throws has still left its declarations behind, and skipping the release on that
            // path is exactly how the next evaluation would inherit them.
            DisposeScope(scope);
        }
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

        var methodProviders = scopedMethodProviders != null ? GlobalMethodProviders.Concat(scopedMethodProviders) : GlobalMethodProviders;
        var scope = engine.CreateScope(methodProviders.SelectMany(x => x.GetMethods()), ShellScope.Services, fileProvider, basePath);

        try
        {
            // Awaited here rather than returned, so that the scope is released after the evaluation has
            // actually finished. An engine cannot be reset while an asynchronous evaluation it started is
            // still outstanding.
            return await engine.EvaluateAsync(scope, script, cancellationToken);
        }
        finally
        {
            DisposeScope(scope);
        }
    }

    public IScriptingEngine GetScriptingEngine(string prefix)
    {
        return _engines.FirstOrDefault(x => x.Prefix == prefix);
    }

    /// <summary>
    /// Releases a scope that holds resources. <see cref="IScriptingScope"/> is a marker interface and most
    /// implementations hold nothing, so the capability is discovered rather than required: adding
    /// <see cref="IDisposable"/> to the interface would break every engine implemented outside this
    /// repository.
    /// </summary>
    private static void DisposeScope(IScriptingScope scope)
        => (scope as IDisposable)?.Dispose();

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
