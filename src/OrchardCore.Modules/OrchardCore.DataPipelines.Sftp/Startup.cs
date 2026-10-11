using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.DataPipelines.Sftp.Drivers;
using OrchardCore.Modules;

namespace OrchardCore.DataPipelines.Sftp;

/// <summary>
/// Registers the step that uploads files to SFTP servers.
/// </summary>
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.TryAddSingleton<IDataPipelineSftpClientFactory, SshNetSftpClientFactory>();
        services.AddDataPipelineStep<UploadToSftpStep, UploadToSftpStepDisplayDriver>();
    }
}
