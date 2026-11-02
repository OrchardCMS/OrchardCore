using Jint;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Rules.Models;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;

namespace OrchardCore.Rules.Services;

public class JavascriptConditionEvaluator : ConditionEvaluator<JavascriptCondition>, IDisposable
{
    private readonly IScriptingManager _scriptingManager;
    private readonly IServiceProvider _serviceProvider;

    // The scope is built lazily once per request.
    private IScriptingScope _scope;
    private IScriptingEngine _engine;
    private bool _disposed;

    // The global variables of the scope as they were before any condition ran. They are put back after
    // every condition, so that what one condition declares, such as a 'const' or a 'var', is not seen by the
    // next one: the conditions of different layers are written and validated independently of each other.
    private Engine _scopeEngine;
    private GlobalSnapshot _globals;
    private bool _globalsUnavailable;

    public JavascriptConditionEvaluator(IScriptingManager scriptingManager, IServiceProvider serviceProvider)
    {
        _scriptingManager = scriptingManager;
        _serviceProvider = serviceProvider;
    }

    public override async ValueTask<bool> EvaluateAsync(JavascriptCondition condition)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _engine ??= _scriptingManager.GetScriptingEngine("js");

        // Conditions are evaluated while a request is being served, so a client that has gone away is the
        // natural point to stop a script that is still running.
        var cancellationToken = _serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext?.RequestAborted ?? CancellationToken.None;

        if (_scope is null)
        {
            var scope = CreateScope();

            if (!TryCaptureGlobals(scope))
            {
                // The globals of this scope cannot be put back after a condition, so sharing it would let the
                // conditions clash. Each condition gets a scope of its own instead, and nothing else holds
                // that scope, so it is released as soon as the condition is done.
                try
                {
                    return Convert.ToBoolean(await _engine.EvaluateAsync(scope, condition.Script, cancellationToken));
                }
                finally
                {
                    (scope as IDisposable)?.Dispose();
                }
            }

            _scope = scope;
        }

        try
        {
            return Convert.ToBoolean(await _engine.EvaluateAsync(_scope, condition.Script, cancellationToken));
        }
        finally
        {
            // Also when the condition failed: a script that throws has still declared its variables.
            _scopeEngine.Advanced.RestoreGlobalSnapshot(_globals);
        }
    }

    private IScriptingScope CreateScope()
        => _engine.CreateScope(_scriptingManager.GlobalMethodProviders.SelectMany(x => x.GetMethods()), _serviceProvider, null, null);

    private bool TryCaptureGlobals(IScriptingScope scope)
    {
        if (_globalsUnavailable || scope is not JavaScriptScope javaScriptScope)
        {
            return false;
        }

        try
        {
            _globals = javaScriptScope.Engine.Advanced.CaptureGlobalSnapshot();
            _scopeEngine = javaScriptScope.Engine;

            return true;
        }
        catch (NotSupportedException)
        {
            // The site configured a global object that keeps its properties itself, out of the engine's reach.
            _globalsUnavailable = true;

            return false;
        }
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

        // The snapshot keeps the engine it was captured from reachable, so it is let go together with the
        // scope. Returning the scope resets the engine to the state it was built in, which is earlier than the
        // snapshot, so nothing the snapshot recorded for this request survives into the next one. Clearing
        // the fields also means a condition still running when the evaluator is disposed fails at its
        // restore, rather than restoring this request's globals onto an engine that may be serving another.
        _globals = null;
        _scopeEngine = null;

        (_scope as IDisposable)?.Dispose();
        _scope = null;
    }
}
