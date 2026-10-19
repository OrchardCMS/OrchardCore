using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.DataLocalization.Deployment;

public class AllDataTranslationsDeploymentStep : DeploymentStep
{
    public AllDataTranslationsDeploymentStep()
    {
        Name = "AllDataTranslations";
        Category = CommonLocalizationSources.Internationalization;
    }
}
