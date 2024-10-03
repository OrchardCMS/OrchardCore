using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Queries.Drivers;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.UrlRewriting.Drivers;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.Options;
using OrchardCore.UrlRewriting.Recipes;
using OrchardCore.UrlRewriting.Services;

namespace OrchardCore.UrlRewriting;

public sealed class Startup : StartupBase
{
    public override int Order
        => OrchardCoreConstants.ConfigureOrder.UrlRewriting;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IRewriteRulesStore, RewriteRulesStore>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddScoped<IDisplayDriver<RewriteRule>, RewriteRulesDisplayDriver>();

        services.AddPermissionProvider<UrlRewritingPermissionProvider>();
        services.AddTransient<IConfigureOptions<RewriteOptions>, RewriteOptionsConfiguration>();
        services.AddRecipeExecutionStep<UrlRewritingStep>();
        services.AddScoped<IRewriteRulesManager, RewriteRulesManager>();

        services.AddRewriteRuleSource<UrlRedirectRuleSource>(UrlRedirectRuleSource.SourceName)
            .AddScoped<IDisplayDriver<RewriteRule>, UrlRedirectRuleDisplayDriver>();

        services.AddRewriteRuleSource<UrlRewriteRuleSource>(UrlRewriteRuleSource.SourceName)
            .AddScoped<IDisplayDriver<RewriteRule>, UrlRewriteRuleDisplayDriver>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        var rewriteOptions = serviceProvider.GetRequiredService<IOptions<RewriteOptions>>().Value;
        app.UseRewriter(rewriteOptions);
    }
}
