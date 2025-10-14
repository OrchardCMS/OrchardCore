using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Azure.Core;
using OrchardCore.Modules;

namespace OrchardCore.Azure;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddAzureOptions();
    }
}
