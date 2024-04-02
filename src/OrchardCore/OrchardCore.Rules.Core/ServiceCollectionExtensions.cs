using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRule<TCondition, TEvaluator>(this IServiceCollection services)
        where TCondition : Condition, new()
        where TEvaluator : IConditionEvaluator
        => services.AddCondition<TCondition, TEvaluator, ConditionFactory<TCondition>>()
            .AddJsonDerivedTypeInfo<TCondition, Condition>();

    public static IServiceCollection AddRule<TCondition, TEvaluator, TDisplayDriver>(this IServiceCollection services)
        where TCondition : Condition, new()
        where TEvaluator : IConditionEvaluator
        where TDisplayDriver : DisplayDriver<Condition, TCondition>
        => services.AddRule<TCondition, TEvaluator>()
            .AddScoped<IDisplayDriver<Condition>, TDisplayDriver>();

    public static IServiceCollection AddConditionOperators(this IServiceCollection services)
    {
        // Allows to serialize 'ConditionOperator' derived types
        services.AddConditionOperator<StringEqualsOperator>();
        services.AddConditionOperator<StringNotEqualsOperator>();
        services.AddConditionOperator<StringStartsWithOperator>();
        services.AddConditionOperator<StringNotStartsWithOperator>();
        services.AddConditionOperator<StringEndsWithOperator>();
        services.AddConditionOperator<StringNotEndsWithOperator>();
        services.AddConditionOperator<StringContainsOperator>();
        services.AddConditionOperator<StringNotContainsOperator>();

        return services;
    }

    public static IServiceCollection AddConditionOperator<TOperator>(this IServiceCollection services)
        where TOperator : ConditionOperator
        => services.AddJsonDerivedTypeInfo<TOperator, ConditionOperator>();
}
