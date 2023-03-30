using System.Threading.Tasks;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public class AnyConditionEvaluator : ConditionEvaluator<AnyConditionGroup>
    {
        private readonly IConditionResolver _conditionResolver;

        public AnyConditionEvaluator(IConditionResolver conditionResolver)
        {
            _conditionResolver = conditionResolver;
        }

        public async override ValueTask<bool> EvaluateAsync(AnyConditionGroup condition)
        {
            foreach (var childCondition in condition.Conditions)
            {
                var evaluator = _conditionResolver.GetConditionEvaluator(childCondition);
                if (await evaluator.EvaluateAsync(childCondition))
                {
                    return true;
                }
            }

            return false;
        }
    }
}