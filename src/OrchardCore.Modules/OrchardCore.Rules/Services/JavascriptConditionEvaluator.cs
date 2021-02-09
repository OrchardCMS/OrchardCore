using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Rules.Models;
using OrchardCore.Scripting;

namespace OrchardCore.Rules.Services
{
    public class JavascriptConditionEvaluator : ConditionEvaluator<JavascriptCondition>
    {
        private readonly IScriptingManager _scriptingManager;

        // The scope is built lazily once per request.
        private IScriptingScope _scope;
        private IScriptingEngine _engine;

        public JavascriptConditionEvaluator(IScriptingManager scriptingManager)
        {
            _scriptingManager = scriptingManager;
        }

        public override ValueTask<bool> EvaluateAsync(JavascriptCondition condition)
        {
            _engine ??= _scriptingManager.GetScriptingEngine("js");
            _scope ??= _engine.CreateScope(_scriptingManager.GlobalMethodProviders.SelectMany(x => x.GetMethods()), ShellScope.Services, null, null);
            
            return new ValueTask<bool>(Convert.ToBoolean(_engine.Evaluate(_scope, condition.Script)));
        }
    }
}