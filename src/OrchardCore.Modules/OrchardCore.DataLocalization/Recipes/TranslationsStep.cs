using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.DataLocalization.Models;
using OrchardCore.DataLocalization.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.DataLocalization.Recipes;

public class TranslationsStep : IRecipeStepHandler
{
    private readonly TranslationsManager _translationsManager;

    public TranslationsStep(TranslationsManager translationsManager)
    {
        _translationsManager = translationsManager;
    }

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, "DynamicDataTranslations", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var model = context.Step.ToObject<TranslationsStepModel>();

        foreach (var importedTranslation in model.Translations)
        {
            if (string.IsNullOrWhiteSpace(importedTranslation.Name))
            {
                continue;
            }

            await _translationsManager.UpdateTranslationAsync(importedTranslation.Name, new[] { importedTranslation.Translation }).ConfigureAwait(false);
        }
    }

    public class TranslationsStepModel
    {
        public TranslationsStepRoleModel[] Translations { get; set; }
    }
}

public class TranslationsStepRoleModel
{
    public string Name { get; set; }

    public Translation Translation { get; set; }
}
