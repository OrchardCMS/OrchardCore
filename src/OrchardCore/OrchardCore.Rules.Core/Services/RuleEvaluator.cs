using System.Threading.Tasks;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public interface IRuleEvaluator
    {
        ValueTask<bool> EvaluateAsync(Rule rule);
    }    

    public abstract class RuleEvaluator<T> : IRuleEvaluator where T : Rule
    {
        ValueTask<bool> IRuleEvaluator.EvaluateAsync(Rule rule)
            => EvaluateAsync(rule as T);

        public abstract ValueTask<bool> EvaluateAsync(T rule);
    }
}