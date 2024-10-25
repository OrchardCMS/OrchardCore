using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Facebook.Drivers;
using OrchardCore.Facebook.Filters;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Facebook;

[Feature(FacebookConstants.Features.Pixel)]
public sealed class StartupPixel : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteDisplayDriver<FacebookPixelSettingsDisplayDriver>();
        services.AddPermissionProvider<PixelPermissionProvider>();
        services.AddNavigationProvider<AdminMenuPixel>();

        services.Configure<MvcOptions>((options) =>
        {
            options.Filters.Add<FacebookPixelFilter>();
        });
    }
}
