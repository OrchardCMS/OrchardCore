using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.DataPipelines.Drivers;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.Modules;

namespace OrchardCore.DataPipelines;

/// <summary>
/// Registers the steps that deliver files to FTP and SFTP servers and to web APIs.
/// </summary>
[Feature("OrchardCore.DataPipelines")]
public sealed class RemoteDestinationsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient(SendToWebApiStep.HttpClientName);
        services.TryAddSingleton<IDataPipelineFtpClientFactory, FluentFtpClientFactory>();
        services.TryAddSingleton<IDataPipelineSftpClientFactory, SshNetSftpClientFactory>();

        services.AddDataPipelineStep<UploadToFtpStep, UploadToFtpStepDisplayDriver>();
        services.AddDataPipelineStep<UploadToSftpStep, UploadToSftpStepDisplayDriver>();
        services.AddDataPipelineStep<SendToWebApiStep, SendToWebApiStepDisplayDriver>();
    }
}

/// <summary>
/// Registers the step that sends files by email.
/// </summary>
[Feature("OrchardCore.DataPipelines.Email")]
public sealed class EmailDestinationStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataPipelineStep<SendByEmailStep, SendByEmailStepDisplayDriver>();
    }
}
