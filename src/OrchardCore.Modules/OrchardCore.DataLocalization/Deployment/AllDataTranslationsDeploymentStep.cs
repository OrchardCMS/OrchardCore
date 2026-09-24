using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.DataLocalization.Deployment;

public class AllDataTranslationsDeploymentStep : DeploymentStep
{
    public AllDataTranslationsDeploymentStep()
    {
        Name = "AllDataTranslations";
        Category = LocalizedString.Create("Internationalization");
    }
}
