using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Rules.Drivers;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.Services;

namespace OrchardCore.Rules;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions<ConditionOptions>();

        // Rule services.
        services
            .AddDisplayDriver<Rule, RuleDisplayDriver>()
            .AddSingleton<IConditionIdGenerator, ConditionIdGenerator>()
            .AddTransient<IConfigureOptions<ConditionOperatorOptions>, ConditionOperatorConfigureOptions>()
            .AddScoped<IConditionResolver, ConditionResolver>()
            .AddScoped<IConditionOperatorResolver, ConditionOperatorResolver>()
            .AddScoped<IRuleService, RuleService>();

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

        // JavaScript condition.
        services.AddRule<JavascriptCondition, JavascriptConditionEvaluator, JavascriptConditionDisplayDriver>();

        // Is authenticated condition.
        services.AddRule<IsAuthenticatedCondition, IsAuthenticatedConditionEvaluator, IsAuthenticatedConditionDisplayDriver>();

        // Is anonymous condition.
        services.AddRule<IsAnonymousCondition, IsAnonymousConditionEvaluator, IsAnonymousConditionDisplayDriver>();

        // Content type condition.
        services.AddDisplayDriver<Condition, ContentTypeConditionDisplayDriver>()
            .AddRuleCondition<ContentTypeCondition, ContentTypeConditionEvaluatorDriver>()
            .AddScoped<IContentDisplayDriver>(sp => sp.GetRequiredService<ContentTypeConditionEvaluatorDriver>());

        // Allows to serialize 'ConditionOperator' derived types
        services.AddRuleConditionOperator<StringEqualsOperator>()
            .AddRuleConditionOperator<StringNotEqualsOperator>()
            .AddRuleConditionOperator<StringStartsWithOperator>()
            .AddRuleConditionOperator<StringNotStartsWithOperator>()
            .AddRuleConditionOperator<StringEndsWithOperator>()
            .AddRuleConditionOperator<StringNotEndsWithOperator>()
            .AddRuleConditionOperator<StringContainsOperator>()
            .AddRuleConditionOperator<StringNotContainsOperator>();
    }
}
