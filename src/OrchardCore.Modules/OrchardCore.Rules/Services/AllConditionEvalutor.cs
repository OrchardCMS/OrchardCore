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

        public override ValueTask<bool> EvaluateAsync(AllConditionGroup condition)
        {
            foreach(var childCondition in condition.Conditions)
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

            if (condition.Conditions.Any())
            {
                return True;
            }

            // This rule requires all conditions to be evaluated as true.
            return False;
        }        
    }
}
