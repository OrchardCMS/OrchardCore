using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Recipes.Services;
using OrchardCore.ResourceManagement;
using OrchardCore.Roles;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings.Deployment;
using OrchardCore.Settings.Drivers;
using OrchardCore.Settings.Recipes;
using OrchardCore.Settings.Services;
using OrchardCore.Setup.Events;

namespace OrchardCore.Settings;

/// <summary>
/// These services are registered on the tenant service collection.
/// </summary>
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TemplateOptions>(o =>
        {
            o.Scope.SetValue("Site", new ObjectValue(new LiquidSiteSettingsAccessor()));
            o.MemberAccessStrategy.Register<LiquidSiteSettingsAccessor, FluidValue>(async (obj, name, context) =>
            {
                var liquidTemplateContext = (LiquidTemplateContext)context;

                var siteService = liquidTemplateContext.Services.GetRequiredService<ISiteService>();
                var site = await siteService.GetSiteSettingsAsync();

                FluidValue result = name switch
                {
                    nameof(ISite.SiteName) => new StringValue(site.SiteName),
                    nameof(ISite.PageTitleFormat) => new StringValue(site.PageTitleFormat),
                    nameof(ISite.SiteSalt) => new StringValue(site.SiteSalt),
                    nameof(ISite.SuperUser) => new StringValue(site.SuperUser),
                    nameof(ISite.Calendar) => new StringValue(site.Calendar),
                    nameof(ISite.TimeZoneId) => new StringValue(site.TimeZoneId),
                    nameof(ISite.ResourceDebugMode) => new StringValue(site.ResourceDebugMode.ToString()),
                    nameof(ISite.UseCdn) => BooleanValue.Create(site.UseCdn),
                    nameof(ISite.CdnBaseUrl) => new StringValue(site.CdnBaseUrl),
                    nameof(ISite.PageSize) => NumberValue.Create(site.PageSize),
                    nameof(ISite.MaxPageSize) => NumberValue.Create(site.MaxPageSize),
                    nameof(ISite.MaxPagedCount) => NumberValue.Create(site.MaxPagedCount),
                    nameof(ISite.BaseUrl) => new StringValue(site.BaseUrl),
                    nameof(ISite.HomeRoute) => new ObjectValue(site.HomeRoute),
                    nameof(ISite.AppendVersion) => BooleanValue.Create(site.AppendVersion),
                    nameof(ISite.CacheMode) => new StringValue(site.CacheMode.ToString()),
                    nameof(ISite.Properties) => new ObjectValue(site.Properties),
                    _ => NilValue.Instance
                };

                return result;
            });
        });

        services.AddScoped<ISetupEventHandler, SetupEventHandler>();
        services.AddPermissionProvider<Permissions>();

        services.AddRolesCoreServices()
            .AddScoped<IAuthorizationHandler, SuperUserHandler>();

        services.AddRecipeExecutionStep<SettingsStep>();
        services.AddSingleton<ISiteService, SiteService>();

        // Site Settings editor
        services.AddSiteDisplayDriver<DefaultSiteSettingsDisplayDriver>();
        services.AddSiteDisplayDriver<ButtonsSettingsDisplayDriver>();
        services.AddNavigationProvider<AdminMenu>();

        services.AddScoped<ITimeZoneSelector, DefaultTimeZoneSelector>();

        services.AddDeployment<SiteSettingsDeploymentSource, SiteSettingsDeploymentStep, SiteSettingsDeploymentStepDriver>();

        services.AddScoped<IRecipeEnvironmentProvider, RecipeEnvironmentSiteNameProvider>();

        services.AddTransient<IPostConfigureOptions<ResourceOptions>, ResourceOptionsConfiguration>();
        services.AddTransient<IPostConfigureOptions<PagerOptions>, PagerOptionsConfiguration>();
    }
}
