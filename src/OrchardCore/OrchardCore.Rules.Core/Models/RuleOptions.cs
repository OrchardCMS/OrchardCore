using System;
using System.Collections.Generic;
using OrchardCore.Rules.Services;

namespace OrchardCore.Rules.Models
{
    public class RuleOptions
    {
        private readonly Dictionary<Type, Type> _evaluators = new Dictionary<Type, Type>();
        public IReadOnlyDictionary<Type, Type> Evaluators => _evaluators;

        private readonly Dictionary<Type, Type> _comparers = new Dictionary<Type, Type>();
        public IReadOnlyDictionary<Type, Type> Comparers => _comparers;

        public List<OperatorOption> Operators { get; set; } = new List<OperatorOption>();
        public List<MethodOption> MethodOptions { get; set; } = new List<MethodOption>();

        internal void AddRuleMethod(Type ruleMethod, Type ruleMethodEvaluator)
        {
            if (!typeof(Rule).IsAssignableFrom(ruleMethod))
            {
                throw new ArgumentException("The type must implement " + nameof(Rule));
            }   

            if (!typeof(IRuleEvaluator).IsAssignableFrom(ruleMethodEvaluator))
            {
                throw new ArgumentException("The type must implement " + nameof(ruleMethodEvaluator));
            }   

            _evaluators[ruleMethod] = ruleMethodEvaluator;                     
        }

       internal void AddRuleOperator(Type op, Type operatorComparer)
        {
            if (!typeof(Operator).IsAssignableFrom(op))
            {
                throw new ArgumentException("The type must implement " + nameof(Operator));
            }   

            if (!typeof(IOperatorComparer).IsAssignableFrom(operatorComparer))
            {
                throw new ArgumentException("The type must implement " + nameof(IOperatorComparer));
            }   

            _comparers[op] = operatorComparer;                     
        }        
    }



    // this might be enough to describe an operator.
    public class OperatorOption
    {
        public string DisplayText { get; set; }
        public IOperatorComparer Comparer { get; set; }
        public Type Operator { get; set; }
    }

    // Methods will be localized by drivers.

    public class MethodOption
    {
        public Type Type { get; set; }
        public List<OperatorOption> Operators { get; set; }
    }
}