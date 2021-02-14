using System.Linq;
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

        public override ValueTask<bool> EvaluateAsync(AnyConditionGroup condition)
        {
            foreach(var childCondition in condition.Conditions)
            {
                var evaluator = _conditionResolver.GetConditionEvaluator(childCondition);
                var task = evaluator.EvaluateAsync(childCondition);
                if (!task.IsCompletedSuccessfully)
                {
                    if(Awaited(task).Result)
                    {
                        return True;
                    }
                }
                else if (task.Result)
                {
                    return True;
                }
            }

            return False;
        }
    }
}
