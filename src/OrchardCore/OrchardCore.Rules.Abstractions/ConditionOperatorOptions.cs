using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Rules
{
    public class ConditionOperatorOptions
    {

        private Dictionary<string, IConditionOperatorFactory> _factories;
        public IReadOnlyDictionary<string, IConditionOperatorFactory> Factories => _factories ??= Operators.ToDictionary(x => x.Factory.Name, x => x.Factory);

        private Dictionary<Type, ConditionOperatorOption> _conditionOperatorOptionByType;
        public IReadOnlyDictionary<Type, ConditionOperatorOption> ConditionOperatorOptionByType => _conditionOperatorOptionByType ??= Operators.ToDictionary(x => x.Operator, x => x);

        public List<ConditionOperatorOption> Operators { get; set; } = new();
    }

    public class ConditionOperatorOption<TLocalizer> : ConditionOperatorOption where TLocalizer : class
    {
        public ConditionOperatorOption(
            Func<IStringLocalizer, LocalizedString> displayText,
            IOperatorComparer comparer,
            Type operatorType,
            IConditionOperatorFactory factory) : base(
                (sp) => displayText((IStringLocalizer)sp.GetService(typeof(IStringLocalizer<>).MakeGenericType(typeof(TLocalizer)))),
                comparer,
                operatorType,
                factory)
        {
        }
    }

    public class ConditionOperatorOption
    {
        public ConditionOperatorOption(
            Func<IServiceProvider, LocalizedString> displayText,
            IOperatorComparer comparer,
            Type operatorType,
            IConditionOperatorFactory factory)
        {
            DisplayText = displayText;
            Comparer = comparer;
            Operator = operatorType;
            Factory = factory;
        }

        public Func<IServiceProvider, LocalizedString> DisplayText { get; }
        public IOperatorComparer Comparer { get; }
        public Type Operator { get; }
        public IConditionOperatorFactory Factory { get; }
    }
}
