using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Routing;
using OrchardCore.Security.Permissions;
using OrchardCore.Seo;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Cache;
using OrchardCore.Sitemaps.Deployment;
using OrchardCore.Sitemaps.Drivers;
using OrchardCore.Sitemaps.Handlers;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Recipes;
using OrchardCore.Sitemaps.Routing;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataMigration<Migrations>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddPermissionProvider<Permissions>();

        services.Configure<SitemapsOptions>(options =>
        {
            if (options.GlobalRouteValues.Count == 0)
            {
                options.GlobalRouteValues = new RouteValueDictionary
                {
                    {"Area", "OrchardCore.Sitemaps"},
                    {"Controller", "Sitemap"},
                    {"Action", "Index"}
                };

                options.SitemapIdKey = "sitemapId";
            }
        });

        services.AddSingleton<SitemapEntries>();
        services.AddSingleton<ISitemapManager, SitemapManager>();
        services.AddSingleton<IShellRouteValuesAddressScheme, SitemapValuesAddressScheme>();
        services.AddSingleton<SitemapRouteTransformer>();

        services.AddScoped<ISitemapIdGenerator, SitemapIdGenerator>();
        services.AddScoped<ISitemapHelperService, SitemapHelperService>();
        services.AddScoped<ISitemapBuilder, DefaultSitemapBuilder>();
        services.AddScoped<ISitemapTypeBuilder, SitemapTypeBuilder>();
        services.AddScoped<ISitemapCacheProvider, DefaultSitemapCacheProvider>();
        services.AddScoped<ISitemapUpdateHandler, DefaultSitemapUpdateHandler>();
        services.AddScoped<ISitemapTypeUpdateHandler, SitemapTypeUpdateHandler>();
        services.AddScoped<ISitemapTypeBuilder, SitemapIndexTypeBuilder>();
        services.AddScoped<ISitemapTypeUpdateHandler, SitemapIndexTypeUpdateHandler>();
        services.AddScoped<ISitemapModifiedDateProvider, DefaultSitemapModifiedDateProvider>();
        services.AddScoped<IRouteableContentTypeCoordinator, DefaultRouteableContentTypeCoordinator>();

        // Sitemap Part.
        services.AddContentPart<SitemapPart>()
            .UseDisplayDriver<SitemapPartDisplayDriver>()
            .AddHandler<SitemapPartHandler>();

        // Custom sitemap path.
        services.AddScoped<ISitemapSourceBuilder, CustomPathSitemapSourceBuilder>();
        services.AddScoped<ISitemapSourceUpdateHandler, CustomPathSitemapSourceUpdateHandler>();
        services.AddScoped<ISitemapSourceModifiedDateProvider, CustomPathSitemapSourceModifiedDateProvider>();
        services.AddDisplayDriver<SitemapSource, CustomPathSitemapSourceDriver>();
        services.AddScoped<ISitemapSourceFactory, SitemapSourceFactory<CustomPathSitemapSource>>();

        services.AddRecipeExecutionStep<SitemapsStep>();

        // Allows to serialize 'SitemapType' derived types.
        services.AddJsonDerivedTypeInfo<Sitemap, SitemapType>();
        services.AddJsonDerivedTypeInfo<SitemapIndex, SitemapType>();

        // Allows to serialize 'SitemapSource' derived types.
        services.AddJsonDerivedTypeInfo<ContentTypesSitemapSource, SitemapSource>();
        services.AddJsonDerivedTypeInfo<CustomPathSitemapSource, SitemapSource>();
        services.AddJsonDerivedTypeInfo<SitemapIndexSource, SitemapSource>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapDynamicControllerRoute<SitemapRouteTransformer>("/{**sitemap}");
    }
}

[Feature("OrchardCore.Sitemaps.RazorPages")]
public sealed class SitemapsRazorPagesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions<SitemapsRazorPagesOptions>();
        services.AddScoped<IRouteableContentTypeProvider, RazorPagesContentTypeProvider>();
    }
}

[Feature("OrchardCore.Sitemaps.Cleanup")]
public sealed class SitemapsCleanupStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IBackgroundTask, SitemapCacheBackgroundTask>();
    }
}

[RequireFeatures("OrchardCore.Deployment", "OrchardCore.Sitemaps")]
public sealed class SitemapsDeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<AllSitemapsDeploymentSource, AllSitemapsDeploymentStep, AllSitemapsDeploymentStepDriver>();
    }
}

[RequireFeatures("OrchardCore.Seo")]
public sealed class SeoStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IRobotsProvider, SitemapsRobotsProvider>();
        services.AddSiteDisplayDriver<SitemapsRobotsSettingsDisplayDriver>();
    }
}
