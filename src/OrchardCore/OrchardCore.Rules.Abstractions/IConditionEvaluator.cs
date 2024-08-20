namespace OrchardCore.Rules;

public interface IConditionEvaluator
{
    ValueTask<bool> EvaluateAsync(Condition condition);
}
