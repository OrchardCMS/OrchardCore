using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.DataLocalization.Deployment;

public class AllDataTranslationsDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<AllDataTranslationsDeploymentStep>("Internationalization");

    public AllDataTranslationsDeploymentStep()
    {
        Name = "AllDataTranslations";
        Category = s_category;
    }
}
