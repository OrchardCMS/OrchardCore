using System;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.Services;

namespace OrchardCore.Rules
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRuleMethod(this IServiceCollection services, Type ruleMethod, Type ruleMethodEvaluator)
        {
            services.Configure<RuleOptions>(o =>
            {
                o.AddRuleMethod(ruleMethod, ruleMethodEvaluator);
            });

            services.AddTransient(ruleMethodEvaluator);

            return services;
        }

        public static IServiceCollection AddRuleMethod<TMethod, TMethodEvaluator>(this IServiceCollection services)
            where TMethod : Rule
            where TMethodEvaluator : IRuleEvaluator
            => services.AddRuleMethod(typeof(TMethod), typeof(TMethodEvaluator));

        public static IServiceCollection AddRuleOperator(this IServiceCollection services, Type op, Type comparer)
        {
            services.Configure<RuleOptions>(o =>
            {
                o.AddRuleOperator(op, comparer);
            });

            // TODO probably we just instantiate an instance here. there's no real di required.

            services.AddTransient(comparer);

            return services;
        }      


        public static IServiceCollection AddRuleOperator<TOperator, TOperatorComparer>(this IServiceCollection services)
            where TOperator : Operator
            where TOperatorComparer : IOperatorComparer
            => services.AddRuleOperator(typeof(TOperator), typeof(TOperatorComparer));              
    }
}