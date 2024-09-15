using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Modules;
using OrchardCore.UrlRewriting.Drivers;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.UrlRewriting.Options;

namespace OrchardCore.UrlRewriting;

public sealed class Startup : StartupBase
{
    public override int Order
        => OrchardCoreConstants.ConfigureOrder.UrlRewriting;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddNavigationProvider<AdminMenu>();
        services.AddSiteDisplayDriver<UrlRewritingSettingsDisplayDriver>();
        services.AddPermissionProvider<UrlRewritingPermissionProvider>();
        services.AddTransient<IConfigureOptions<RewriteOptions>, RewriteOptionsConfiguration>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        var rewriteOptions = serviceProvider.GetRequiredService<IOptions<RewriteOptions>>().Value;
        app.UseRewriter(rewriteOptions);
    }
}
