using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.Rules;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRule<TCondition, TEvaluator, TDisplayDriver>(this IServiceCollection services)
        where TCondition : Condition, new()
        where TEvaluator : class, IConditionEvaluator
        where TDisplayDriver : class, IDisplayDriver<Condition>
    {
        return services.AddRuleCondition<TCondition, TEvaluator>()
                .AddDisplayDriver<Condition, TDisplayDriver>();
    }

    public static IServiceCollection AddRuleConditionOperator<TOperator>(this IServiceCollection services)
        where TOperator : ConditionOperator
    {
        return services.AddJsonDerivedTypeInfo<TOperator, ConditionOperator>();
    }

    public static IServiceCollection AddRuleCondition<TCondition, TConditionEvaluator>(this IServiceCollection services)
        where TCondition : Condition, new()
        where TConditionEvaluator : class, IConditionEvaluator
    {
        return services.AddRuleCondition<TCondition, TConditionEvaluator, ConditionFactory<TCondition>>();
    }
}
