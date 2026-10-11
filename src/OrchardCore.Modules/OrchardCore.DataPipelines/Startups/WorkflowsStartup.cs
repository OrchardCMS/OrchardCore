using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Workflows;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.DataPipelines.Startups;

[Feature("OrchardCore.DataPipelines.Workflows")]
public sealed class WorkflowsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddActivity<RunDataPipelineTask, RunDataPipelineTaskDisplayDriver>();
        services.AddActivity<DataPipelineRunCompletedEvent, DataPipelineRunCompletedEventDisplayDriver>();
        services.AddScoped<IDataPipelineRunHandler, WorkflowDataPipelineRunHandler>();
    }
}
