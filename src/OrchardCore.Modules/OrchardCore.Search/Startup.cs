using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Search.Deployment;
using OrchardCore.Search.Drivers;
using OrchardCore.Search.Migrations;
using OrchardCore.Search.Models;
using OrchardCore.Search.ViewModels;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddSiteDisplayDriver<SearchSettingsDisplayDriver>();
    }
}

[RequireFeatures("OrchardCore.Contents")]
public sealed class ContentsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataMigration<SearchMigrations>();

        services
            .AddContentPart<SearchFormPart>()
            .UseDisplayDriver<SearchFormPartDisplayDriver>();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<SearchSettingsDeploymentSource, SearchSettingsDeploymentStep, SearchSettingsDeploymentStepDriver>();
    }
}

[RequireFeatures("OrchardCore.Liquid")]
public sealed class LiquidStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TemplateOptions>(o =>
        {
            o.MemberAccessStrategy.Register<SearchIndexViewModel>();
            o.MemberAccessStrategy.Register<SearchFormViewModel>();
            o.MemberAccessStrategy.Register<SearchResultsViewModel>();
        });
    }
}
