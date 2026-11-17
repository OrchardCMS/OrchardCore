using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Scope;
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
        services.AddScoped<ISetupEventHandler, SetupEventHandler>();
        services.AddPermissionProvider<Permissions>();

        services.AddRolesCoreServices()
            .AddScoped<IAuthorizationHandler, SuperUserHandler>()
            .AddScoped<IAuthorizationHandler, SiteSettingsAuthorizationHandler>();

        services.AddRecipeExecutionStep<SettingsStep>();
        services.AddSingleton<ISiteService, SiteService>();

        // Site Settings editor
        services.AddSiteDisplayDriver<DefaultSiteSettingsDisplayDriver>();
        services.AddSiteDisplayDriver<DebugSettingsDisplayDriver>();
        services.AddSiteDisplayDriver<ButtonsSettingsDisplayDriver>();
        services.AddSiteSettingsPermission(DefaultSiteSettingsDisplayDriver.GroupId, SettingsPermissions.ManageGeneralSettings);
        services.AddSiteSettingsPermission(DebugSettingsDisplayDriver.GroupId, SettingsPermissions.ManageDebuggingSettings);
        services.AddNavigationProvider<AdminMenu>();

        services.AddScoped<ITimeZoneSelector, DefaultTimeZoneSelector>();

        services.AddDeployment<SiteSettingsDeploymentSource, SiteSettingsDeploymentStep, SiteSettingsDeploymentStepDriver>();

        services.AddScoped<IRecipeEnvironmentProvider, RecipeEnvironmentSiteNameProvider>();
        services.AddSignalOptionsChangeTokenSource<ShapeRenderingOptions>();

        services.AddTransient<IPostConfigureOptions<ResourceOptions>, ResourceOptionsConfiguration>();
        services.AddTransient<IPostConfigureOptions<PagerOptions>, PagerOptionsConfiguration>();
        services.AddTransient<IConfigureOptions<ShapeRenderingOptions>, ShapeRenderingOptionsConfiguration>();

        services.AddScoped<IModularTenantEvents, PreloadSiteSettingsTenantEventHandler>();
    }
}

[RequireFeatures("OrchardCore.Liquid.Core")]
public sealed class LiquidStartup : StartupBase
{
    private readonly IShellConfiguration _configuration;

    public LiquidStartup(IShellConfiguration configuration) =>
        _configuration = configuration;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<SettingsLiquidOptions>(_configuration.GetSection("Liquid"));
        
        services.AddSingleton<ISitePropertiesLiquidMapper, SitePropertiesLiquidMapper>();
        services.Configure<TemplateOptionsBuilder>(o =>
        {
            o.WithGlobalValue("Site", ObjectValue.Create(new LiquidSiteSettingsAccessor()));
            o.ConfigureMemberAccess(strategy => strategy.Register<LiquidSiteSettingsAccessor, FluidValue>(async (obj, name, context) =>
            {
                var liquidTemplateContext = (LiquidTemplateContext)context;
                var services = liquidTemplateContext.Services;

                var siteService = services.GetRequiredService<ISiteService>();
                var site = await siteService.GetSiteSettingsAsync();

                FluidValue result = name switch
                {
                    nameof(ISite.SiteName) => StringValue.Create(site.SiteName ?? string.Empty),
                    nameof(ISite.PageTitleFormat) => StringValue.Create(site.PageTitleFormat ?? string.Empty),
                    // The site salt should never be accessible to Liquid and exposing it is a major security risk. This
                    // comment and the dummy value below should be kept, to record that it's intentional.
                    nameof(ISite.SiteSalt) => StringValue.Create("[REDACTED]"),
                    nameof(ISite.SuperUser) => StringValue.Create(site.SuperUser ?? string.Empty),
                    nameof(ISite.Calendar) => StringValue.Create(site.Calendar ?? string.Empty),
                    nameof(ISite.TimeZoneId) => StringValue.Create(site.TimeZoneId ?? string.Empty),
                    nameof(ISite.ResourceDebugMode) => StringValue.Create(site.ResourceDebugMode.ToString()),
                    nameof(ISite.UseCdn) => BooleanValue.Create(site.UseCdn),
                    nameof(ISite.CdnBaseUrl) => StringValue.Create(site.CdnBaseUrl ?? string.Empty),
                    nameof(ISite.PageSize) => NumberValue.Create(site.PageSize),
                    nameof(ISite.MaxPageSize) => NumberValue.Create(site.MaxPageSize),
                    nameof(ISite.MaxPagedCount) => NumberValue.Create(site.MaxPagedCount),
                    nameof(ISite.BaseUrl) => StringValue.Create(site.BaseUrl ?? string.Empty),
                    nameof(ISite.HomeRoute) => ObjectValue.Create(site.HomeRoute),
                    nameof(ISite.AppendVersion) => BooleanValue.Create(site.AppendVersion),
                    nameof(ISite.CacheMode) => StringValue.Create(site.CacheMode.ToString()),
                    nameof(ISite.Properties) => await services.GetRequiredService<ISitePropertiesLiquidMapper>().MapAsync(site),
                    _ => NilValue.Instance
                };

                return result;
            }));
        });
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteSettingsPropertyDeploymentStep<DebugSettings, DeploymentStartup>(
            S => S["Debugging settings"],
            S => S["Exports the debugging settings."]);
    }
}
