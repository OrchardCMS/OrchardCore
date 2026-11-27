using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.DataLocalization.Deployment;

public class AllDataTranslationsDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "AllDataTranslations";

    public static LocalizationSource Category => DataLocalizationLocalizationSources.Internationalization;
}
