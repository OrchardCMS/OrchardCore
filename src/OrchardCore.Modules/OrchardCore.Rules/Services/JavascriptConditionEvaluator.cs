using Jint;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Rules.Models;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;

namespace OrchardCore.Rules.Services;

public class JavascriptConditionEvaluator : ConditionEvaluator<JavascriptCondition>
{
    private readonly IScriptingManager _scriptingManager;
    private readonly IServiceProvider _serviceProvider;

    // The scope is built lazily once per request.
    private IScriptingScope _scope;
    private IScriptingEngine _engine;

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
                // conditions clash. Each condition gets a scope of its own instead.
                return Convert.ToBoolean(await _engine.EvaluateAsync(scope, condition.Script, cancellationToken));
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
}
