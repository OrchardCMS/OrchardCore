using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public class AllConditionEvaluator : ConditionEvaluator<AllConditionGroup>
    {
        private readonly IConditionResolver _conditionResolver;

        public AllConditionEvaluator(IConditionResolver conditionResolver)
        {
            _conditionResolver = conditionResolver;
        }

        public async override ValueTask<bool> EvaluateAsync(AllConditionGroup condition)
        {
            foreach (var childCondition in condition.Conditions)
            {
                var evaluator = _conditionResolver.GetConditionEvaluator(childCondition);
                if (!await evaluator.EvaluateAsync(childCondition))
                {
                    return false;
                }
            }

            if (condition.Conditions.Any())
            {
                return true;
            }

            // This rule requires all conditions to be evaluated as true.
            return false;
        }
    }
}