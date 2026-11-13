using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DataPipelines.Drivers;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.Modules;

namespace OrchardCore.DataPipelines;

/// <summary>
/// Registers the step that sends files to web APIs.
/// </summary>
[Feature(DataPipelinesConstants.FeatureId)]
public sealed class RemoteDestinationsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient(SendToWebApiStep.HttpClientName);
        services.AddDataPipelineStep<SendToWebApiStep, SendToWebApiStepDisplayDriver>();
    }
}

/// <summary>
/// Registers the steps that send files, or links to them, by email.
/// </summary>
[RequireFeatures("OrchardCore.Email")]
public sealed class EmailDestinationStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataPipelineStep<SendByEmailStep, SendByEmailStepDisplayDriver>();
        services.AddDataPipelineStep<ShareDownloadLinkStep, ShareDownloadLinkStepDisplayDriver>();
    }
}
