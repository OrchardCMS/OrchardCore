using System;
using System.Collections.Generic;

namespace OrchardCore.Rules
{
    public class ConditionOptions
    {
        private readonly Dictionary<Type, Type> _evaluators = new();
        public IReadOnlyDictionary<Type, Type> Evaluators => _evaluators;

        internal void AddCondition(Type condition, Type conditionEvaluator)
        {
            if (!typeof(Condition).IsAssignableFrom(condition))
            {
                throw new ArgumentException("The type must implement " + nameof(Condition));
            }

            if (!typeof(IConditionEvaluator).IsAssignableFrom(conditionEvaluator))
            {
                throw new ArgumentException("The type must implement " + nameof(conditionEvaluator));
            }

            _evaluators[condition] = conditionEvaluator;
        }
    }
}
