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
        var directiveIndex = directive.IndexOf(':');
        if (directiveIndex == -1 || directiveIndex >= directive.Length - 2)
        {
            return directive;
        }

        var prefix = directive[..directiveIndex];
        var script = directive[(directiveIndex + 1)..];

        var engine = GetScriptingEngine(prefix);
        if (engine == null)
        {
            return directive;
        }

        var methodProviders = scopedMethodProviders != null ? GlobalMethodProviders.Concat(scopedMethodProviders) : GlobalMethodProviders;
        var scope = engine.CreateScope(methodProviders.SelectMany(x => x.GetMethods()), ShellScope.Services, fileProvider, basePath);
        return engine.Evaluate(scope, script);
    }

    public IScriptingEngine GetScriptingEngine(string prefix)
    {
        return _engines.FirstOrDefault(x => x.Prefix == prefix);
    }
}
