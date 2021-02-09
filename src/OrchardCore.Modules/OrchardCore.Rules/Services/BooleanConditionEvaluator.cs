using System.Threading.Tasks;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public class BooleanConditionEvaluator : ConditionEvaluator<BooleanCondition>
    {
        public override ValueTask<bool> EvaluateAsync(BooleanCondition condition)
            => new ValueTask<bool>(condition.Value);
    }
}