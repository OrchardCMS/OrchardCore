using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Rules.Services
{
    public class RuleService : IRuleService
    {
        private static readonly ValueTask<bool> False = new ValueTask<bool>(false);
        private static readonly ValueTask<bool> True = new ValueTask<bool>(true);

        private readonly IConditionResolver _conditionResolver;

        public RuleService(IConditionResolver conditionResolver)
        {
            _conditionResolver = conditionResolver;
        }

        public ValueTask<bool> EvaluateAsync(Rule rule)
        {          
            static async ValueTask<bool> Awaited(ValueTask<bool> task)
                => await task;
            
            foreach(var childCondition in rule.Conditions)
            {
                var evaluator = _conditionResolver.GetConditionEvaluator(childCondition);

                var task = evaluator.EvaluateAsync(childCondition);
                if (!task.IsCompletedSuccessfully)
                {
                    if (!Awaited(task).Result)
                    {
                        return False;
                    }
                }
                else
                {
                    if (!task.Result)
                    {
                        return False;
                    }
                }
            }

            if (rule.Conditions.Any())
            {
                return True;
            }

            // This rule requires all conditions to be evaluated as true.
            return False;            
        }
    }
}
