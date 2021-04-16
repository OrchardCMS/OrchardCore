using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Rules.Drivers;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.Services;

namespace OrchardCore.Rules
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions<ConditionOptions>();

            // Rule services.
            services.AddScoped<IDisplayManager<Condition>, DisplayManager<Condition>>()
                .AddScoped<IDisplayManager<Rule>, DisplayManager<Rule>>()
                .AddTransient<IDisplayDriver<Rule>, RuleDisplayDriver>()
                .AddSingleton<IConditionIdGenerator, ConditionIdGenerator>()
                .AddTransient<IConfigureOptions<ConditionOperatorOptions>, ConditionOperatorConfigureOptions>()
                .AddScoped<IConditionResolver, ConditionResolver>()
                .AddScoped<IConditionOperatorResolver, ConditionOperatorResolver>()
                .AddScoped<IRuleService, RuleService>()
                .AddScoped<IRuleMigrator, RuleMigrator>();

            // All condition.
            services
                .AddTransient<IDisplayDriver<Condition>, AllConditionDisplayDriver>()
                .AddCondition<AllConditionGroup, AllConditionEvaluator, ConditionFactory<AllConditionGroup>>();

            // Any condition.
            services
                .AddTransient<IDisplayDriver<Condition>, AnyConditionDisplayDriver>()
                .AddCondition<AnyConditionGroup, AnyConditionEvaluator, ConditionFactory<AnyConditionGroup>>();

            // Boolean condition.
            services
                .AddTransient<IDisplayDriver<Condition>, BooleanConditionDisplayDriver>()
                .AddCondition<BooleanCondition, BooleanConditionEvaluator, ConditionFactory<BooleanCondition>>();

            // Homepage condition.
            services
                .AddTransient<IDisplayDriver<Condition>, HomepageConditionDisplayDriver>()
                .AddCondition<HomepageCondition, HomepageConditionEvaluator, ConditionFactory<HomepageCondition>>();

            // Url condition.
            services
                .AddTransient<IDisplayDriver<Condition>, UrlConditionDisplayDriver>()
                .AddCondition<UrlCondition, UrlConditionEvaluator, ConditionFactory<UrlCondition>>();

            // Culture condition.
            services
                .AddTransient<IDisplayDriver<Condition>, CultureConditionDisplayDriver>()
                .AddCondition<CultureCondition, CultureConditionEvaluator, ConditionFactory<CultureCondition>>();

            // Role condition.
            services
                .AddTransient<IDisplayDriver<Condition>, RoleConditionDisplayDriver>()
                .AddCondition<RoleCondition, RoleConditionEvaluator, ConditionFactory<RoleCondition>>();

            // Javascript condition.
            services
                .AddTransient<IDisplayDriver<Condition>, JavascriptConditionDisplayDriver>()
                .AddCondition<JavascriptCondition, JavascriptConditionEvaluator, ConditionFactory<JavascriptCondition>>();

            // Is authenticated condition.
            services
                .AddTransient<IDisplayDriver<Condition>, IsAuthenticatedConditionDisplayDriver>()
                .AddCondition<IsAuthenticatedCondition, IsAuthenticatedConditionEvaluator, ConditionFactory<IsAuthenticatedCondition>>();

            // Is anonymous condition.
            services
                .AddTransient<IDisplayDriver<Condition>, IsAnonymousConditionDisplayDriver>()
                .AddCondition<IsAnonymousCondition, IsAnonymousConditionEvaluator, ConditionFactory<IsAnonymousCondition>>();

            // Content type condition.
            services
                .AddTransient<IDisplayDriver<Condition>, ContentTypeConditionDisplayDriver>()
                .AddCondition<ContentTypeCondition, ContentTypeConditionEvaluatorDriver, ConditionFactory<ContentTypeCondition>>()
                .AddTransient<IContentDisplayDriver>(sp => (IContentDisplayDriver)sp.GetRequiredService<ContentTypeConditionEvaluatorDriver>());
        }
    }
}
