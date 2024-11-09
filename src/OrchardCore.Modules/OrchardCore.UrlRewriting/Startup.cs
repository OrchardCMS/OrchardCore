using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.ResourceManagement;
using OrchardCore.Security.Permissions;
using OrchardCore.UrlRewriting.Drivers;
using OrchardCore.UrlRewriting.Endpoints.Rules;
using OrchardCore.UrlRewriting.Extensions;
using OrchardCore.UrlRewriting.Handlers;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.Recipes;
using OrchardCore.UrlRewriting.Services;

namespace OrchardCore.UrlRewriting;

public sealed class Startup : StartupBase
{
    public override int Order
        => OrchardCoreConstants.ConfigureOrder.UrlRewriting;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddUrlRewritingServices()
            .AddNavigationProvider<AdminMenu>()
            .AddPermissionProvider<UrlRewritingPermissionProvider>()
            .AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>()
            .AddDisplayDriver<RewriteRule, RewriteRulesDisplayDriver>();

        // Add Apache Mod Redirect Rule.
        services.AddRewriteRuleSource<UrlRedirectRuleSource>(UrlRedirectRuleSource.SourceName)
            .AddScoped<IRewriteRuleHandler, UrlRedirectRuleHandler>()
            .AddDisplayDriver<RewriteRule, UrlRedirectRuleDisplayDriver>();

        // Add Apache Mod Rewrite Rule.
        services.AddRewriteRuleSource<UrlRewriteRuleSource>(UrlRewriteRuleSource.SourceName)
            .AddScoped<IRewriteRuleHandler, UrlRewriteRuleHandler>()
            .AddDisplayDriver<RewriteRule, UrlRewriteRuleDisplayDriver>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.AddSortRulesEndpoint();

        app.UseUrlRewriting(serviceProvider);
    }
}

[RequireFeatures("OrchardCore.Recipes.Core")]
public sealed class RecipesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddRecipeExecutionStep<UrlRewritingStep>();
    }
}
