using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public class AllRuleEvaluator : RuleEvaluator<AllRule>
    {      
        private readonly IRuleResolver _ruleResolver;
        
        public AllRuleEvaluator(IRuleResolver ruleResolver)
        {
            _ruleResolver = ruleResolver;
        }

        public async override ValueTask<bool> EvaluateAsync(AllRule method)
        {
            foreach(var child in method.Children)
            {
                var evaluator = _ruleResolver.GetRuleEvaluator(child);
                if (!await evaluator.EvaluateAsync(child))
                {
                    return false;
                }
            }

            if (method.Children.Any())
            {
                return true;
            }

            // This rule requires children to be evaluated as true.
            return false;
        }
    }
}