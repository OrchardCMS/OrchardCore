using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DataPipelines.Drivers;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.Modules;

namespace OrchardCore.DataPipelines.Startups;

[Feature("OrchardCore.DataPipelines.Media")]
public sealed class MediaStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataPipelineStep<ReadMediaFileStep, ReadMediaFileStepDisplayDriver>();
        services.AddDataPipelineStep<SaveToMediaStep, SaveToMediaStepDisplayDriver>();
    }
}
