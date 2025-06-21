using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.DataLocalization.Services;
using OrchardCore.Deployment;

namespace OrchardCore.DataLocalization.Deployment;

public class AllDataTranslationsDeploymentSource : IDeploymentSource
{
    private readonly TranslationsManager _translationsManager;

    public AllDataTranslationsDeploymentSource(TranslationsManager translationsManager)
    {
        _translationsManager = translationsManager ?? throw new ArgumentNullException(nameof(translationsManager));
    }

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        var allDataTranslationsState = step as AllDataTranslationsDeploymentStep;

        if (allDataTranslationsState == null)
        {
            return;
        }

        var translationObjects = new JsonArray();
        var translationsDocument = await _translationsManager.GetTranslationsDocumentAsync().ConfigureAwait(false);

        foreach (var translation in translationsDocument.Translations)
        {
            translationObjects[translation.Key] = JObject.FromObject(translation.Value);
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = "DynamicDataTranslations",
            ["DynamicDataTranslations"] = translationObjects,
        });

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
