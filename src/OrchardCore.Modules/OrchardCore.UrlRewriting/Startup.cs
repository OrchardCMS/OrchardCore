using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Modules;
using OrchardCore.UrlRewriting.Rules;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.Drivers;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.UrlRewriting;

public sealed class Startup : StartupBase
{
    public override int Order
        => OrchardCoreConstants.ConfigureOrder.UrlRewriting;

    private readonly AdminOptions _adminOptions;

    public Startup(IOptions<AdminOptions> adminOptions)
    {
        _adminOptions = adminOptions.Value;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddNavigationProvider<AdminMenu>();
        services.AddSiteDisplayDriver<UrlRewritingSettingsDisplayDriver>();
        services.AddPermissionProvider<Permissions>();
    }

    public override async ValueTask ConfigureAsync(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        var siteService = app.ApplicationServices.GetRequiredService<ISiteService>();
        var modRewriteSettings = await siteService.GetSettingsAsync<UrlRewritingSettings>();

        using var apacheModRewrite = new StringReader(modRewriteSettings.ApacheModRewrite ?? string.Empty);

        var options = new RewriteOptions()
            .AddApacheModRewrite(apacheModRewrite);

        if (options.Rules.Count > 0)
        {
            // Exclude admin ui requests to prevent accidental access bricking by provided rules
            options.Rules.Insert(0, new ExcludeAdminUIRule(_adminOptions.AdminUrlPrefix));
            app.UseRewriter(options);
        }
    }
}
