using System.Text.Json.Nodes;
using OrchardCore.DataLocalization.Services;
using OrchardCore.DataLocalization.ViewModels;
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
        var translationsDocument = await _translationsManager.GetTranslationsDocumentAsync();

        foreach (var translation in translationsDocument.Translations)
        {
            translationObjects.Add(JObject.FromObject(new TranslationsViewModel
            {
                Key = translation.Key,
                Translations = translation.Value,
            }));
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = "DynamicDataTranslations",
            ["DynamicDataTranslations"] = translationObjects,
        });

        await Task.CompletedTask;
    }
}
