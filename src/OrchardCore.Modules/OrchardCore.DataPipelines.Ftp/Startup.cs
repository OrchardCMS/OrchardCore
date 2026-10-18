using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.DataPipelines.Ftp.Drivers;
using OrchardCore.Modules;

namespace OrchardCore.DataPipelines.Ftp;

/// <summary>
/// Registers the step that uploads files to FTP servers.
/// </summary>
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.TryAddSingleton<IDataPipelineFtpClientFactory, FluentFtpClientFactory>();
        services.AddDataPipelineStep<UploadToFtpStep, UploadToFtpStepDisplayDriver>();
    }
}
