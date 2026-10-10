using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DataPipelines.Deployment;
using OrchardCore.Deployment;
using OrchardCore.Modules;

namespace OrchardCore.DataPipelines.Startups;

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
        => services.AddDeployment<AllDataPipelinesDeploymentSource, AllDataPipelinesDeploymentStep, AllDataPipelinesDeploymentStepDriver>();
}
