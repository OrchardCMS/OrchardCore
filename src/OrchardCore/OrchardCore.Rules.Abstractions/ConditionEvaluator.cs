using System.Threading.Tasks;

namespace OrchardCore.Rules
{
    public abstract class ConditionEvaluator<T> : IConditionEvaluator where T : Condition
    {
        protected static ValueTask<bool> False => new(false);
        protected static ValueTask<bool> True => new(true);

        ValueTask<bool> IConditionEvaluator.EvaluateAsync(Condition condition)
            => EvaluateAsync(condition as T);

        public abstract ValueTask<bool> EvaluateAsync(T condition);
    }
}
