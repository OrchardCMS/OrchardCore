using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes.TheAgencyTheme;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();
    }
}
