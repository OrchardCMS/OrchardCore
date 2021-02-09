using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.DisplayManagement;
using OrchardCore.Rules.Drivers;
using OrchardCore.Rules;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.Services;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using Orchard.Rules.Drivers;

namespace OrchardCore.Rules
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions<ConditionOptions>();

            services
                .AddScoped<IDisplayManager<Rule>, DisplayManager<Rule>>()
                .AddScoped<IDisplayDriver<Rule>, RuleDisplayDriver>();

            // All condition.
            services
                .AddScoped<IDisplayDriver<Condition>, AllConditionDisplayDriver>()
                .AddCondition<AllConditionGroup, AllConditionEvaluator, ConditionFactory<AllConditionGroup>>();

            // Any condition.
            services
                .AddScoped<IDisplayDriver<Condition>, AnyConditionDisplayDriver>()
                .AddCondition<AnyConditionGroup, AnyConditionEvaluator, ConditionFactory<AnyConditionGroup>>();

            // Boolean condition.
            services
                .AddScoped<IDisplayDriver<Condition>, BooleanConditionDisplayDriver>()
                .AddCondition<BooleanCondition, BooleanConditionEvaluator, ConditionFactory<BooleanCondition>>();

            // Homepage condition.
            services
                .AddScoped<IDisplayDriver<Condition>, HomepageConditionDisplayDriver>()
                .AddCondition<HomepageCondition, HomepageConditionEvaluator, ConditionFactory<HomepageCondition>>();

            // Url condition.
            services
                .AddScoped<IDisplayDriver<Condition>, UrlConditionDisplayDriver>()
                .AddCondition<UrlCondition, UrlConditionEvaluator, ConditionFactory<UrlCondition>>();

            // Javascript condition.
            services
                .AddScoped<IDisplayDriver<Condition>, JavascriptConditionDisplayDriver>()
                .AddCondition<JavascriptCondition, JavascriptConditionEvaluator, ConditionFactory<JavascriptCondition>>();

            // Is authenticated condition.
            services
                .AddScoped<IDisplayDriver<Condition>, IsAuthenticatedConditionDisplayDriver>()
                .AddCondition<IsAuthenticatedCondition, IsAuthenticatedConditionEvaluator, ConditionFactory<IsAuthenticatedCondition>>();

            // Is anonymous condition.
            services
                .AddScoped<IDisplayDriver<Condition>, IsAnonymousConditionDisplayDriver>()
                .AddCondition<IsAnonymousCondition, IsAnonymousConditionEvaluator, ConditionFactory<IsAnonymousCondition>>();                       

            // Content type condition.
            services
                .AddScoped<IDisplayDriver<Condition>, ContentTypeConditionDisplayDriver>()
                .AddCondition<ContentTypeCondition, ContentTypeConditionEvaluator, ConditionFactory<ContentTypeCondition>>()                       
                .AddScoped<IDisplayedContentItemDriver, DisplayedContentTypeDriver>()
                .AddScoped<IContentDisplayDriver>(sp => sp.GetRequiredService<IDisplayedContentItemDriver>() as IContentDisplayDriver);

            services.AddScoped<IDisplayManager<Condition>, DisplayManager<Condition>>();

            services.AddSingleton<IConditionIdGenerator, ConditionIdGenerator>();
            services.AddTransient<IConfigureOptions<ConditionOperatorOptions>, ConditionOperatorConfigureOptions>();

            services.AddScoped<IConditionResolver, ConditionResolver>();
            services.AddScoped<IConditionOperatorResolver, ConditionOperatorResolver>();

            services.AddScoped<IRuleService, RuleService>();

            services.AddScoped<IRuleMigrator, RuleMigrator>();
        }
    }
}

