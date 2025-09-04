using OrchardCore.Rules.Models;
using OrchardCore.Scripting;

namespace OrchardCore.Rules.Services;

public class JavascriptConditionEvaluator : ConditionEvaluator<JavascriptCondition>
{
    private readonly IScriptingManager _scriptingManager;
    private readonly IServiceProvider _serviceProvider;

    // The scope is built lazily once per request.
    private IScriptingScope _scope;
    private IScriptingEngine _engine;


    public JavascriptConditionEvaluator(IScriptingManager scriptingManager, IServiceProvider serviceProvider)
    {
        _scriptingManager = scriptingManager;
        _serviceProvider = serviceProvider;
    }

    public override ValueTask<bool> EvaluateAsync(JavascriptCondition condition)
    {
        _engine ??= _scriptingManager.GetScriptingEngine("js");
        _scope ??= _engine.CreateScope(_scriptingManager.GlobalMethodProviders.SelectMany(x => x.GetMethods()), _serviceProvider, null, null);

        return Convert.ToBoolean(_engine.Evaluate(_scope, condition.Script)) ? True : False;
    }
}
