using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public class AllRuleGroupEvaluator<T> : RuleEvaluator<T> where T : AllRule
    {      
        private readonly IRuleResolver _ruleResolver;
        
        public AllRuleGroupEvaluator(IRuleResolver ruleResolver)
        {
            _ruleResolver = ruleResolver;
        }

        public async override ValueTask<bool> EvaluateAsync(T rule)
        {
            foreach(var child in rule.Rules)
            {
                var evaluator = _ruleResolver.GetRuleEvaluator(child);
                if (!await evaluator.EvaluateAsync(child))
                {
                    return false;
                }
            }

            if (rule.Rules.Any())
            {
                return true;
            }

            // This rule requires children to be evaluated as true.
            return false;
        }
    }
}