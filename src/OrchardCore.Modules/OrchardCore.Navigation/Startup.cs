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
        services.AddShapeTableProvider<PagerShapesTableProvider>();
        services.AddShapeAttributes<PagerShapes>();

        var navigationConfiguration = _shellConfiguration.GetSection("OrchardCore_Navigation");
        services.Configure<PagerOptions>(navigationConfiguration.GetSection("PagerOptions"));
    }
}
