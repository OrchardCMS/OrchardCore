using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.Rules;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRule<TCondition, TEvaluator, TDisplayDriver>(this IServiceCollection services)
        where TCondition : Condition, new()
        where TEvaluator : IConditionEvaluator
        where TDisplayDriver : class, IDisplayDriver<Condition>
        => services.AddRuleCondition<TCondition, TEvaluator>()
            .AddScoped<IDisplayDriver<Condition>, TDisplayDriver>();

    public static IServiceCollection AddRuleConditionOperator<TOperator>(this IServiceCollection services)
        where TOperator : ConditionOperator
        => services.AddJsonDerivedTypeInfo<TOperator, ConditionOperator>();

    public static IServiceCollection AddRuleCondition<TCondition, TConditionEvaluator>(this IServiceCollection services)
        where TCondition : Condition, new()
        => services.AddCondition(typeof(TCondition), typeof(TConditionEvaluator), typeof(ConditionFactory<TCondition>))
        .AddJsonDerivedTypeInfo<TCondition, Condition>();

    public static IServiceCollection AddRuleCondition<TCondition, TConditionEvaluator, TConditionFactory>(this IServiceCollection services)
        where TCondition : Condition
        where TConditionEvaluator : IConditionEvaluator
        where TConditionFactory : IConditionFactory
        => services.AddCondition(typeof(TCondition), typeof(TConditionEvaluator), typeof(TConditionFactory))
        .AddJsonDerivedTypeInfo<TCondition, Condition>();
}
