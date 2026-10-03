using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Scripting;

public interface IScriptingEngine
{
    string Prefix { get; }
    object Evaluate(IScriptingScope scope, string script);
    Task<object> EvaluateAsync(IScriptingScope scope, string script, CancellationToken cancellationToken = default)
        => Task.FromResult(Evaluate(scope, script));

    IScriptingScope CreateScope(IEnumerable<GlobalMethod> methods, IServiceProvider serviceProvider, IFileProvider fileProvider, string basePath);
}
