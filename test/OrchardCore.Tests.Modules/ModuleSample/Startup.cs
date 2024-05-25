using Microsoft.Extensions.DependencyInjection;
using ModuleSample.Services;
using OrchardCore.Modules;

namespace ModuleSample;
public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<TestLocalizationService>();
    }
}
