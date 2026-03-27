using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.DataLocalization.Models;
using OrchardCore.DataLocalization.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.DataLocalization.Recipes;

/// <summary>
/// This recipe step imports translations.
/// Handles both the new "Translations" format and the legacy "DynamicDataTranslations" format.
/// </summary>
public sealed class TranslationsStep : IRecipeStepHandler
{
    private readonly TranslationsManager _translationsManager;
    private readonly IStringLocalizer S;

    public TranslationsStep(
        TranslationsManager translationsManager,
        IStringLocalizer<TranslationsStep> stringLocalizer)
    {
        _translationsManager = translationsManager;
        S = stringLocalizer;
    }

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (string.Equals(context.Name, "Translations", StringComparison.OrdinalIgnoreCase))
        {
            await HandleNewFormatAsync(context);
        }
        else if (string.Equals(context.Name, "DynamicDataTranslations", StringComparison.OrdinalIgnoreCase))
        {
            await HandleLegacyFormatAsync(context);
        }
    }

    private async Task HandleNewFormatAsync(RecipeExecutionContext context)
    {
        var model = context.Step;
        if (model == null)
        {
            return;
        }

        var translationsArray = model["translations"]?.AsArray();
        if (translationsArray == null)
        {
            context.Errors.Add(S["The 'translations' property is required."]);
            return;
        }

        // Group translations by culture.
        var translationsByCulture = new Dictionary<string, List<Translation>>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in translationsArray)
        {
            if (item is not JsonObject translationObj)
            {
                continue;
            }

            var culture = translationObj["culture"]?.GetValue<string>();
            var translationContext = translationObj["context"]?.GetValue<string>();
            var key = translationObj["key"]?.GetValue<string>();
            var value = translationObj["value"]?.GetValue<string>();

            if (string.IsNullOrEmpty(culture) || string.IsNullOrEmpty(key))
            {
                continue;
            }

            if (!translationsByCulture.TryGetValue(culture, out var list))
            {
                list = [];
                translationsByCulture[culture] = list;
            }

            list.Add(new Translation
            {
                Context = translationContext ?? string.Empty,
                Key = key,
                Value = value ?? string.Empty,
            });
        }

        // Update translations for each culture.
        foreach (var (cultureName, translations) in translationsByCulture)
        {
            // Get existing translations and merge.
            var document = await _translationsManager.GetTranslationsDocumentAsync();
            var existingTranslations = document.Translations.TryGetValue(cultureName, out var existing)
                ? existing.ToDictionary(t => $"{t.Context}|{t.Key}", t => t, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, Translation>(StringComparer.OrdinalIgnoreCase);

            // Merge new translations into existing.
            foreach (var translation in translations)
            {
                var key = $"{translation.Context}|{translation.Key}";
                existingTranslations[key] = translation;
            }

            await _translationsManager.UpdateTranslationAsync(cultureName, existingTranslations.Values);
        }
    }

    private async Task HandleLegacyFormatAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<LegacyTranslationsStepModel>();

        if (model?.Translations == null)
        {
            return;
        }

        foreach (var importedTranslation in model.Translations)
        {
            if (string.IsNullOrWhiteSpace(importedTranslation.Name) || importedTranslation.Translation == null)
            {
                continue;
            }

            await _translationsManager.UpdateTranslationAsync(importedTranslation.Name, [importedTranslation.Translation]);
        }
    }

    private sealed class LegacyTranslationsStepModel
    {
        public LegacyTranslationItem[] Translations { get; set; }
    }

    private sealed class LegacyTranslationItem
    {
        public string Name { get; set; }
        public Translation Translation { get; set; }
    }
}
