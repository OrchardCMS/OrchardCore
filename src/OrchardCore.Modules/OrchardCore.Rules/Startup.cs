using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.DisplayManagement;
using OrchardCore.Rules.Drivers;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.Services;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.Rules
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions<RuleOptions>();

            services
                .AddRuleMethod<UrlRule, UrlRuleEvaluator>()
                .AddRuleOperator<StringEqualsOperator, StringEqualsOperatorComparer>();

            services.AddTransient<IRuleFactory, RuleFactory<UrlRule>>();


            services.AddScoped<IDisplayDriver<Rule>, AllRuleDisplayDriver>();


            services.AddRuleMethod<RuleContainer, RuleContainerEvaluator>()
                .AddRuleMethod<AllRule, AllRuleEvaluator>()
                .AddRuleMethod<BooleanRule, BooleanRuleEvaluator>()
                .AddRuleMethod<IsHomepageRule, IsHomepageRuleEvaluator>();

            services.AddTransient<IRuleFactory, RuleFactory<AllRule>>();
            services.AddTransient<IRuleFactory, RuleFactory<BooleanRule>>();
            services.AddTransient<IRuleFactory, RuleFactory<IsHomepageRule>>();


            services.AddScoped<IDisplayManager<Rule>, DisplayManager<Rule>>();

            services.AddScoped<IDisplayManager<RuleContainer>, DisplayManager<RuleContainer>>();
            services.AddScoped<IDisplayDriver<RuleContainer>, RuleContainerDisplayDriver>();

            services.AddSingleton<IRuleIdGenerator, RuleIdGenerator>();


            services.AddTransient<IRuleResolver, RuleResolver>();
            services.AddTransient<IOperatorResolver, OperatorResolver>();

            services.AddTransient<IRuleService, RuleService>();

            services.AddTransient<AllRuleEvaluator>();
        }
    }
}

