using System.Text.Json.Nodes;
using OrchardCore.DataLocalization.Services;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.DataLocalization.Deployment;

public sealed class TranslationsDeploymentSource : DeploymentSourceBase<TranslationsDeploymentStep>
{
    private readonly TranslationsManager _translationsManager;
    private readonly ILocalizationService _localizationService;

    public TranslationsDeploymentSource(
        TranslationsManager translationsManager,
        ILocalizationService localizationService)
    {
        _translationsManager = translationsManager;
        _localizationService = localizationService;
    }

    protected override async Task ProcessAsync(TranslationsDeploymentStep step, DeploymentPlanResult result)
    {
        var translationsDocument = await _translationsManager.GetTranslationsDocumentAsync();
        var supportedCultures = await _localizationService.GetSupportedCulturesAsync();

        var data = new JsonObject { ["name"] = "Translations" };
        var translationsArray = new JsonArray();

        // Determine which cultures to export.
        var culturesToExport = step.IncludeAll
            ? supportedCultures
            : supportedCultures.Where(c => step.Cultures.Contains(c, StringComparer.OrdinalIgnoreCase)).ToArray();

        foreach (var cultureName in culturesToExport)
        {
            if (!translationsDocument.Translations.TryGetValue(cultureName, out var translations))
            {
                continue;
            }

            // Filter by categories if specified.
            var filteredTranslations = step.Categories.Length > 0
                ? translations.Where(t => step.Categories.Contains(t.Context, StringComparer.OrdinalIgnoreCase))
                : translations;

            foreach (var translation in filteredTranslations)
            {
                var translationObj = new JsonObject
                {
                    ["culture"] = cultureName,
                    ["context"] = translation.Context,
                    ["key"] = translation.Key,
                    ["value"] = translation.Value,
                };

                translationsArray.Add(translationObj);
            }
        }

        data["translations"] = translationsArray;
        result.Steps.Add(data);
    }
}
