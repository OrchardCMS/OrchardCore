using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Facebook.Drivers;
using OrchardCore.Facebook.Filters;
using OrchardCore.Facebook.Recipes;
using OrchardCore.Facebook.Services;
using OrchardCore.Facebook.Settings;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.ResourceManagement;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Facebook;

public sealed class Startup : StartupBase
{
    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        builder.UseMiddleware<ScriptsMiddleware>();
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AdminMenu>();

        services.AddSingleton<IFacebookService, FacebookService>();
        services.AddSiteDisplayDriver<FacebookSettingsDisplayDriver>();
        services.AddRecipeExecutionStep<FacebookSettingsStep>();

        services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();
        services.AddTransient<IConfigureOptions<FacebookSettings>, FacebookSettingsConfiguration>();

        services.Configure<MvcOptions>((options) =>
        {
            options.Filters.Add<FBInitFilter>();
        });
    }
}
