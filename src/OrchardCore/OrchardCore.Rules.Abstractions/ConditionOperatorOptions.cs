using System.Collections.Frozen;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Rules;

public class ConditionOperatorOptions
{
    private FrozenDictionary<string, IConditionOperatorFactory> _factories;
    private FrozenDictionary<Type, ConditionOperatorOption> _conditionOperatorOptionByType;

    public IReadOnlyDictionary<string, IConditionOperatorFactory> Factories
        => _factories ??= Operators.ToFrozenDictionary(x => x.Factory.Name, x => x.Factory);

    public IReadOnlyDictionary<Type, ConditionOperatorOption> ConditionOperatorOptionByType
        => _conditionOperatorOptionByType ??= Operators.ToFrozenDictionary(x => x.Operator, x => x);

    public List<ConditionOperatorOption> Operators { get; set; } = [];
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
