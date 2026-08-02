using OrchardCore.Rules.Models;
using OrchardCore.Scripting;

namespace OrchardCore.Rules.Services;

public class JavascriptConditionEvaluator : ConditionEvaluator<JavascriptCondition>, IDisposable
{
    private readonly IScriptingManager _scriptingManager;
    private readonly IServiceProvider _serviceProvider;

    // The scope is built lazily once per request.
    private IScriptingScope _scope;
    private IScriptingEngine _engine;
    private bool _disposed;

    public JavascriptConditionEvaluator(IScriptingManager scriptingManager, IServiceProvider serviceProvider)
    {
        _scriptingManager = scriptingManager;
        _serviceProvider = serviceProvider;
    }

    public override async ValueTask<bool> EvaluateAsync(JavascriptCondition condition)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _engine ??= _scriptingManager.GetScriptingEngine("js");
        _scope ??= _engine.CreateScope(_scriptingManager.GlobalMethodProviders.SelectMany(x => x.GetMethods()), _serviceProvider, null, null);

        return Convert.ToBoolean(await _engine.EvaluateAsync(_scope, condition.Script));
    }

    /// <summary>
    /// Releases the scope held for the request. This type is registered as a scoped service precisely so
    /// that every condition of a request shares one scope, which makes it the only thing that knows when
    /// that scope ends; the container disposes it at the end of the request.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || _disposed)
        {
            return;
        }

        _disposed = true;

        (_scope as IDisposable)?.Dispose();
        _scope = null;
    }
}
