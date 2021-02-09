using System.Threading.Tasks;

namespace OrchardCore.Rules
{
    public interface IConditionEvaluator
    {
        ValueTask<bool> EvaluateAsync(Condition condition);
    }    

    public abstract class ConditionEvaluator<T> : IConditionEvaluator where T : Condition
    {
        ValueTask<bool> IConditionEvaluator.EvaluateAsync(Condition condition)
            => EvaluateAsync(condition as T);

        public abstract ValueTask<bool> EvaluateAsync(T condition);
    }
}