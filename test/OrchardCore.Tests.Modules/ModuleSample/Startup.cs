using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace ModuleSample;
//[Feature("TestCustomQueryFeature")]
public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
    }
}
