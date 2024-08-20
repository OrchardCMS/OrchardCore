using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Rules;

public static class ServiceCollectionExtensions
{
    [Obsolete("This method is deprecated and will be removed in future releases. Please use the .AddRule() or .AddRuleCondition() extensions found in OrchardCore.Rule.Core instead.")]
    public static IServiceCollection AddCondition(this IServiceCollection services, Type condition, Type conditionEvaluator, Type conditionFactory)
    {
        services.Configure<ConditionOptions>(o =>
        {
            o.AddCondition(condition, conditionEvaluator);
        });

        // Rules are scoped so that during a request rules like the script rule can build the scripting engine once.
        services.AddScoped(conditionEvaluator);

        var factoryDescriptor = new ServiceDescriptor(typeof(IConditionFactory), conditionFactory, ServiceLifetime.Singleton);
        services.Add(factoryDescriptor);

        return services;
    }

    [Obsolete("This method is deprecated and will be removed in future releases. Please use the .AddRule<> or .AddRuleCondition<> extensions found in OrchardCore.Rule.Core instead.")]
    public static IServiceCollection AddCondition<TCondition, TConditionEvaluator, TConditionFactory>(this IServiceCollection services)
        where TCondition : Condition
        where TConditionEvaluator : IConditionEvaluator
        where TConditionFactory : IConditionFactory
    {
        return services.AddCondition(typeof(TCondition), typeof(TConditionEvaluator), typeof(TConditionFactory));
    }

    public static IServiceCollection AddRuleCondition<TCondition, TConditionEvaluator, TConditionFactory>(this IServiceCollection services)
        where TCondition : Condition
        where TConditionEvaluator : class, IConditionEvaluator
        where TConditionFactory : class, IConditionFactory
    {
        services.Configure<ConditionOptions>(o =>
        {
            o.AddCondition(typeof(TCondition), typeof(TConditionEvaluator));
        });

        // Rules are scoped so that during a request rules like the script rule can build the scripting engine once.
        services.AddScoped<TConditionEvaluator>();

        services.AddSingleton<IConditionFactory, TConditionFactory>();

        services.AddJsonDerivedTypeInfo<TCondition, Condition>();

        return services;
    }
}
