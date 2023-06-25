using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Facebook.Drivers;
using OrchardCore.Facebook.Filters;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Facebook;

[Feature(FacebookConstants.Features.Pixel)]
public class StartupPixel : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IDisplayDriver<ISite>, FacebookPixelSettingsDisplayDriver>();
        services.AddScoped<IPermissionProvider, PixelPermissionProvider>();
        services.AddScoped<INavigationProvider, AdminMenuPixel>();

        services.Configure<MvcOptions>((options) =>
        {
            options.Filters.Add(typeof(FacebookPixelFilter));
        });
    }
}
