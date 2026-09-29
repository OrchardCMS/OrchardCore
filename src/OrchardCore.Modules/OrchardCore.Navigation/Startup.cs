using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace OrchardCore.Navigation;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddNavigation();

        services.AddShapeTableProvider<NavigationShapes>();
        services.AddShapeTableProvider<BreadcrumbShapes>();
        services.AddShapeTableProvider<PagerShapesTableProvider>();
        services.AddShapeAttributes<PagerShapes>();

        // The 'OrchardCore_Navigation' section is deprecated and will be removed in a future major version, use 'Navigation' instead.
        services.Configure<PagerOptions>(_shellConfiguration.GetSectionCompat("Navigation:PagerOptions", "OrchardCore_Navigation:PagerOptions"));
    }
}
