using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Forms.Providers;

namespace OrchardCore.Forms.Services
{
    public static class ValidationRuleServiceExtensions
    {
        /// <summary>
        /// Adds validation rule services.
        /// </summary>
        public static IServiceCollection AddValidationRules(this IServiceCollection services)
        {
            services.AddScoped<IValidationRuleService, ValidationRuleService>();

            services.AddSingleton<IValidationRuleProvider, ContainsProvider>();
            services.AddSingleton<IValidationRuleProvider, EqualsProvider>();
            services.AddSingleton<IValidationRuleProvider, IsAfterProvider>();
            services.AddSingleton<IValidationRuleProvider, IsBeforeProvider>();
            services.AddSingleton<IValidationRuleProvider, IsBooleanProvider>();
            services.AddSingleton<IValidationRuleProvider, IsByteLengthProvider>();
            services.AddSingleton<IValidationRuleProvider, IsDateProvider>();
            services.AddSingleton<IValidationRuleProvider, IsDecimalProvider>();
            services.AddSingleton<IValidationRuleProvider, IsDivisibleByProvider>();
            services.AddSingleton<IValidationRuleProvider, IsEmptyProvider>();
            services.AddSingleton<IValidationRuleProvider, IsFloatProvider>();
            services.AddSingleton<IValidationRuleProvider, IsIntProvider>();
            services.AddSingleton<IValidationRuleProvider, IsJSONProvider>();
            services.AddSingleton<IValidationRuleProvider, IsLengthProvider>();
            services.AddSingleton<IValidationRuleProvider, IsNumericProvider>();
            services.AddSingleton<IValidationRuleProvider, MatchesProvider>();
            return services;
        }
    }
}
