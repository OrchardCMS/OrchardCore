using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services;

public class AllConditionEvaluator : ConditionEvaluator<AllConditionGroup>
{
    private readonly IConditionResolver _conditionResolver;

    public AllConditionEvaluator(IConditionResolver conditionResolver)
    {
        _conditionResolver = conditionResolver;
    }

    public override async ValueTask<bool> EvaluateAsync(AllConditionGroup condition)
    {
        foreach (var childCondition in condition.Conditions)
        {
            var evaluator = _conditionResolver.GetConditionEvaluator(childCondition);
            if (evaluator is null || !await evaluator.EvaluateAsync(childCondition))
            {
                return false;
            }
        }

        if (condition.Conditions.Count > 0)
        {
            return true;
        }

        // This rule requires all conditions to be evaluated as true.
        return false;
    }
}
