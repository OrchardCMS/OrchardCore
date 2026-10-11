using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.DataPipelines.Deployment;

/// <summary>
/// Exports every data pipeline, with its published definition, or its draft when it was never published.
/// </summary>
public class AllDataPipelinesDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<AllDataPipelinesDeploymentStep>("Data");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<AllDataPipelinesDeploymentStep>("All Data Pipelines");

    public AllDataPipelinesDeploymentStep()
    {
        Name = "AllDataPipelines";
        Category = s_category;
        Title = s_title;
    }
}
