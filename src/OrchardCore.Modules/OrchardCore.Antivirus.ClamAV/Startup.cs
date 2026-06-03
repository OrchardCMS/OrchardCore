using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage;
using OrchardCore.Antivirus.ClamAV.Services;
using OrchardCore.Modules;

namespace OrchardCore.Antivirus.ClamAV;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _configuration;

    public Startup(IShellConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<ClamAvOptions>(_configuration.GetSection(ClamAvOptions.ConfigSection));

        services.Replace(ServiceDescriptor.Singleton<IAntivirusScanner, ClamAVAntivirusScanner>());
    }
}
