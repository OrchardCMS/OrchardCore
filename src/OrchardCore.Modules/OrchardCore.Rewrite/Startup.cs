using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Rewrite.Rules;
using OrchardCore.Modules;
using OrchardCore.Rewrite.Models;
using OrchardCore.Rewrite.Drivers;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.Rewrite;

public sealed class Startup : StartupBase
{
    public override int Order
        => OrchardCoreConstants.ConfigureOrder.InfrastructureService;

    private readonly AdminOptions _adminOptions;

    public Startup(IOptions<AdminOptions> adminOptions)
    {
        _adminOptions = adminOptions.Value;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<INavigationProvider, AdminMenu>();
        services.AddSiteDisplayDriver<RewriteSettingsDisplayDriver>();
        services.AddScoped<IPermissionProvider, Permissions>();
    }

    public override async ValueTask ConfigureAsync(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        var siteService = app.ApplicationServices.GetRequiredService<ISiteService>();
        var modRewriteSettings = await siteService.GetSettingsAsync<RewriteSettings>();

        var rewriteSettings = modRewriteSettings?.ApacheModRewrite;
        if (rewriteSettings == null)
            return;

        using var apacheModRewrite = new StringReader(rewriteSettings);

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
