using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
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
        services.AddNavigationProvider<AdminMenu>();
        services.AddPermissionProvider<UrlRewritingPermissionProvider>();

        services.AddTransient<IConfigureOptions<RewriteOptions>, RewriteOptionsConfiguration>();
        services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();
        services.AddSingleton<IRewriteRulesStore, RewriteRulesStore>();
        services.AddScoped<IRewriteRulesManager, RewriteRulesManager>();
        services.AddScoped<IRewriteRuleHandler, RewriteRuleHandler>();
        services.AddScoped<IDisplayDriver<RewriteRule>, RewriteRulesDisplayDriver>();

        // Add Apache Mod Rewrite options.
        services.AddRewriteRuleSource<UrlRedirectRuleSource>(UrlRedirectRuleSource.SourceName)
            .AddScoped<IRewriteRuleHandler, UrlRedirectRuleHandler>()
            .AddScoped<IDisplayDriver<RewriteRule>, UrlRedirectRuleDisplayDriver>();
        services.AddRewriteRuleSource<UrlRewriteRuleSource>(UrlRewriteRuleSource.SourceName)
            .AddScoped<IRewriteRuleHandler, UrlRewriteRuleHandler>()
            .AddScoped<IDisplayDriver<RewriteRule>, UrlRewriteRuleDisplayDriver>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.AddSortRulesEndpoint();

        var rewriteOptions = serviceProvider.GetRequiredService<IOptions<RewriteOptions>>().Value;

        app.UseRewriter(rewriteOptions);
    }
}

[Feature("OrchardCore.Recipes.Core")]
public sealed class RecipesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddRecipeExecutionStep<UrlRewritingStep>();
    }
}
