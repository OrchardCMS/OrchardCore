using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Display.ContentDisplay;
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
            services.AddConditionOperators()
                .AddOptions<ConditionOptions>();

            // Rule services.
            services
                .AddScoped<IDisplayDriver<Rule>, RuleDisplayDriver>()
                .AddSingleton<IConditionIdGenerator, ConditionIdGenerator>()
                .AddTransient<IConfigureOptions<ConditionOperatorOptions>, ConditionOperatorConfigureOptions>()
                .AddScoped<IConditionResolver, ConditionResolver>()
                .AddScoped<IConditionOperatorResolver, ConditionOperatorResolver>()
                .AddScoped<IRuleService, RuleService>()
                .AddScoped<IRuleMigrator, RuleMigrator>();

            // All condition.
            services.AddRule<AllConditionGroup, AllConditionEvaluator, AllConditionDisplayDriver>();

            // Any condition.
            services.AddRule<AnyConditionGroup, AnyConditionEvaluator, AnyConditionDisplayDriver>();

            // Boolean condition.
            services.AddRule<BooleanCondition, BooleanConditionEvaluator, BooleanConditionDisplayDriver>();

            // Homepage condition.
            services.AddRule<HomepageCondition, HomepageConditionEvaluator, HomepageConditionDisplayDriver>();

            // Url condition.
            services.AddRule<UrlCondition, UrlConditionEvaluator, UrlConditionDisplayDriver>();

            // Culture condition.
            services.AddRule<CultureCondition, CultureConditionEvaluator, CultureConditionDisplayDriver>();

            // Role condition.
            services.AddRule<RoleCondition, RoleConditionEvaluator, RoleConditionDisplayDriver>();

            // Javascript condition.
            services.AddRule<JavascriptCondition, JavascriptConditionEvaluator, JavascriptConditionDisplayDriver>();

            // Is authenticated condition.
            services.AddRule<IsAuthenticatedCondition, IsAuthenticatedConditionEvaluator, IsAuthenticatedConditionDisplayDriver>();

            // Is anonymous condition.
            services.AddRule<IsAnonymousCondition, IsAnonymousConditionEvaluator, IsAnonymousConditionDisplayDriver>();

            // Content type condition.
            services.AddScoped<IDisplayDriver<Condition>, ContentTypeConditionDisplayDriver>()
                .AddCondition<ContentTypeCondition, ContentTypeConditionEvaluatorDriver>()
                .AddScoped<IContentDisplayDriver>(sp => sp.GetRequiredService<ContentTypeConditionEvaluatorDriver>())
                .AddJsonDerivedTypeInfo<ContentTypeCondition, Condition>();
        }
    }
}
