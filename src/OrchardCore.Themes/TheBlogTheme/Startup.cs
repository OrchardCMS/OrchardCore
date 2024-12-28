using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.Themes.TheBlogTheme;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();
    }
}
