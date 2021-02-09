using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Rules
{
    public class ConditionOperatorOptions
    {

        private Dictionary<Type, IOperatorComparer> _comparers;
        public IReadOnlyDictionary<Type, IOperatorComparer> Comparers => _comparers ??= Operators.ToDictionary(x => x.Operator, x => x.Comparer);

        private Dictionary<string, IConditionOperatorFactory> _factories;
        public IReadOnlyDictionary<string, IConditionOperatorFactory> Factories => _factories ??= Operators.ToDictionary(x => x.Factory.Name, x => x.Factory);


        private Dictionary<Type, IConditionOperatorFactory> _factoriesByType;
        public IReadOnlyDictionary<Type, IConditionOperatorFactory> FactoriesByType => _factoriesByType ??= Operators.ToDictionary(x => x.Operator, x => x.Factory);        

        public List<ConditionOperatorOption> Operators { get; set; } = new List<ConditionOperatorOption>();

    }

    public class ConditionOperatorOption
    {
        public string DisplayText { get; set; }
        public IOperatorComparer Comparer { get; set; }
        public Type Operator { get; set; }
        public IConditionOperatorFactory Factory { get; set; }
    }
}