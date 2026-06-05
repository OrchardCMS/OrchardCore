using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Antivirus.ClamAV;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage;
using OrchardCore.Modules;

namespace OrchardCore.Antivirus;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
    }
}

[Feature("OrchardCore.Antivirus.ClamAV")]
public sealed class ClamAVStartup : StartupBase
{
    private readonly IShellConfiguration _configuration;

    public ClamAVStartup(IShellConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<ClamAvOptions>(_configuration.GetSection(ClamAvOptions.ConfigSection));
        services.TryAddSingleton<ClamAvConnectionFactory>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IFileEventHandler, ClamAvFileEventHandler>());
    }
}
