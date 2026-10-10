using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DataPipelines.Drivers;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.Modules;

namespace OrchardCore.DataPipelines.Startups;

[Feature("OrchardCore.DataPipelines.Contents")]
public sealed class ContentsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataPipelineStep<SaveContentItemsStep, SaveContentItemsStepDisplayDriver>();
    }
}
