using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Rules
{
    public class ConditionOperatorOptions
    {

        private Dictionary<string, IConditionOperatorFactory> _factories;
        public IReadOnlyDictionary<string, IConditionOperatorFactory> Factories => _factories ??= Operators.ToDictionary(x => x.Factory.Name, x => x.Factory);

        private Dictionary<Type, ConditionOperatorOption> _conditionOperatorOptionByType;
        public IReadOnlyDictionary<Type, ConditionOperatorOption> ConditionOperatorOptionByType => _conditionOperatorOptionByType ??= Operators.ToDictionary(x => x.Operator, x => x);        

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
