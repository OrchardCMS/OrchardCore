namespace OrchardCore.Rules.Services;

public interface IRuleService
{
    ValueTask<bool> EvaluateAsync(Rule rule);
}
