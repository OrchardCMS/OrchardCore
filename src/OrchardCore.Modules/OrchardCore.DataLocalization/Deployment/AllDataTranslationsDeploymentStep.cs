using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.DataLocalization.Deployment;

public class AllDataTranslationsDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Internationalization", typeof(AllDataTranslationsDeploymentStep));

    public AllDataTranslationsDeploymentStep()
    {
        Name = "AllDataTranslations";
        Category = s_category;
    }
}
