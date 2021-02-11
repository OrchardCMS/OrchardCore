using System;
using System.Collections.Generic;

namespace OrchardCore.Rules
{
    public class ConditionOptions
    {
        private readonly Dictionary<Type, Type> _evaluators = new Dictionary<Type, Type>();
        public IReadOnlyDictionary<Type, Type> Evaluators => _evaluators;
        private readonly Dictionary<Type, Type> _factories = new Dictionary<Type, Type>();
        public IReadOnlyDictionary<Type, Type> Factories => _factories;

        internal void AddCondition(Type condition, Type conditionEvaluator, Type conditionFactory)
        {
            if (!typeof(Condition).IsAssignableFrom(condition))
            {
                throw new ArgumentException("The type must implement " + nameof(Condition));
            }   

            if (!typeof(IConditionEvaluator).IsAssignableFrom(conditionEvaluator))
            {
                throw new ArgumentException("The type must implement " + nameof(conditionEvaluator));
            }  

            if (!typeof(IConditionFactory).IsAssignableFrom(conditionFactory))
            {
                throw new ArgumentException("The type must implement " + nameof(conditionFactory));
            }             

            _evaluators[condition] = conditionEvaluator;      
            _factories[condition] = conditionFactory;                   
        }
    }
}