using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Facebook.Drivers;
using OrchardCore.Facebook.Endpoints;
using OrchardCore.Facebook.Filters;
using OrchardCore.Facebook.Recipes;
using OrchardCore.Facebook.Services;
using OrchardCore.Facebook.Settings;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Facebook;

public sealed class Startup : StartupBase
{
    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.AddSdkEndpoints();
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AdminMenu>();

        services.AddSingleton<IFacebookService, FacebookService>();
        services.AddSiteDisplayDriver<FacebookSettingsDisplayDriver>();
#pragma warning disable CS0618 // Type or member is obsolete
        services.AddRecipeExecutionStep<FacebookSettingsStep>();
#pragma warning restore CS0618 // Type or member is obsolete

        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();
        services.AddTransient<IConfigureOptions<FacebookSettings>, FacebookSettingsConfiguration>();

        services.Configure<MvcOptions>((options) =>
        {
            options.Filters.Add<FBInitFilter>();
        });
    }
}
