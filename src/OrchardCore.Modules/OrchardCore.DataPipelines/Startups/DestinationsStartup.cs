using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DataPipelines.Drivers;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.Modules;

namespace OrchardCore.DataPipelines;

/// <summary>
/// Registers the step that sends files to web APIs.
/// </summary>
[Feature("OrchardCore.DataPipelines")]
public sealed class RemoteDestinationsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient(SendToWebApiStep.HttpClientName);
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
