using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace ModuleSample;
[Feature("TestCustomQuery")]
public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
    }
}
