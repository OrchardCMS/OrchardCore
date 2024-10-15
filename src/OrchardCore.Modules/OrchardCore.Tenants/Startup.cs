using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Distributed;
using OrchardCore.Modules;
using OrchardCore.Modules.FileProviders;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Setup;
using OrchardCore.Tenants.Deployment;
using OrchardCore.Tenants.Recipes;
using OrchardCore.Tenants.Services;

namespace OrchardCore.Tenants;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddNavigationProvider<AdminMenu>();
        services.AddPermissionProvider<Permissions>();
        services.AddScoped<ITenantValidator, TenantValidator>();
        services.AddShapeTableProvider<TenantShapeTableProvider>();
        services.AddSetup();

        services.Configure<TenantsOptions>(_shellConfiguration.GetSection("OrchardCore_Tenants"));
    }
}

[Feature("OrchardCore.Tenants.FileProvider")]
public sealed class FileProviderStartup : StartupBase
{
    /// <summary>
    /// The path in the tenant's App_Data folder containing the files.
    /// </summary>
    private const string AssetsPath = "wwwroot";

    // Run after other middlewares.
    public override int Order => 10;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ITenantFileProvider>(serviceProvider =>
        {
            var shellOptions = serviceProvider.GetRequiredService<IOptions<ShellOptions>>();
            var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();

            var contentRoot = GetContentRoot(shellOptions.Value, shellSettings);

            if (!Directory.Exists(contentRoot))
            {
                Directory.CreateDirectory(contentRoot);
            }
            return new TenantFileProvider(contentRoot);
        });

        services.AddSingleton<IStaticFileProvider>(serviceProvider =>
        {
            return serviceProvider.GetRequiredService<ITenantFileProvider>();
        });
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        var tenantFileProvider = serviceProvider.GetRequiredService<ITenantFileProvider>();

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = tenantFileProvider,
            DefaultContentType = "application/octet-stream",
            ServeUnknownFileTypes = true,

            // Cache the tenant static files for 30 days.
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers[HeaderNames.CacheControl] = $"public, max-age={TimeSpan.FromDays(30).TotalSeconds}, s-max-age={TimeSpan.FromDays(365.25).TotalSeconds}";
            }
        });
    }

    private static string GetContentRoot(ShellOptions shellOptions, ShellSettings shellSettings) =>
        Path.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellSettings.Name, AssetsPath);
}

[Feature("OrchardCore.Tenants.Distributed")]
public sealed class DistributedStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<DistributedShellMarkerService>();
    }
}

[Feature("OrchardCore.Tenants.FeatureProfiles")]
public sealed class FeatureProfilesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddNavigationProvider<FeatureProfilesAdminMenu>();
        services.AddScoped<FeatureProfilesManager>();
        services.AddScoped<IFeatureProfilesService, FeatureProfilesService>();
        services.AddScoped<IFeatureProfilesSchemaService, FeatureProfilesSchemaService>();
        services.AddShapeTableProvider<TenantFeatureProfileShapeTableProvider>();

        services.AddRecipeExecutionStep<FeatureProfilesStep>();
    }
}

[RequireFeatures("OrchardCore.Deployment", "OrchardCore.Tenants.FeatureProfiles")]
public sealed class FeatureProfilesDeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<AllFeatureProfilesDeploymentSource, AllFeatureProfilesDeploymentStep, AllFeatureProfilesDeploymentStepDriver>();
    }
}

[RequireFeatures("OrchardCore.Features")]
public sealed class TenantFeatureProfilesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddShapeTableProvider<TenantFeatureShapeTableProvider>();
    }
}
