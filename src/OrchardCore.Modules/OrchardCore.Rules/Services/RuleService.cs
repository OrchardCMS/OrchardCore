using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Rules.Services
{
    public class RuleService : IRuleService
    {
        private readonly IConditionResolver _conditionResolver;

        public RuleService(IConditionResolver conditionResolver)
        {
            _conditionResolver = conditionResolver;
        }

        public async ValueTask<bool> EvaluateAsync(Rule rule)
        {
            foreach (var childCondition in rule.Conditions)
            {
                var evaluator = _conditionResolver.GetConditionEvaluator(childCondition);
                if (!await evaluator.EvaluateAsync(childCondition))
                {
                    return false;
                }
            }

            if (rule.Conditions.Any())
            {
                return true;
            }

            // This rule requires all conditions to be evaluated as true.
            return false;
        }
    }
}