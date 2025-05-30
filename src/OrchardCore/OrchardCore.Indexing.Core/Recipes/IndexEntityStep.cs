using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Indexing.Models;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Indexing.Core.Recipes;

public sealed class IndexEntityStep : NamedRecipeStepHandler
{
    public const string StepKey = "Indexing";

    private readonly IIndexEntityManager _indexEntityManager;
    private readonly IndexingOptions _indexingOptions;

    internal readonly IStringLocalizer S;

    public IndexEntityStep(
        IIndexEntityManager indexEntityManager,
        IOptions<IndexingOptions> indexingOptions,
        IStringLocalizer<IndexEntityStep> stringLocalizer)
        : base(StepKey)
    {
        _indexEntityManager = indexEntityManager;
        _indexingOptions = indexingOptions.Value;
        S = stringLocalizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<IndexEntityStepModel>();
        var tokens = model.Indexes.Cast<JsonObject>() ?? [];

        foreach (var token in tokens)
        {
            IndexEntity index = null;

            var id = token[nameof(index.Id)]?.GetValue<string>();

            if (!string.IsNullOrEmpty(id))
            {
                index = await _indexEntityManager.FindByIdAsync(id);
            }

            if (index is not null)
            {
                await _indexEntityManager.UpdateAsync(index, token);
            }
            else
            {
                var providerName = token[nameof(index.ProviderName)]?.GetValue<string>();

                if (string.IsNullOrEmpty(providerName))
                {
                    context.Errors.Add(S["Could not find provider value. The index will not be imported"]);

                    continue;
                }

                var type = token[nameof(index.Type)]?.GetValue<string>();

                if (string.IsNullOrEmpty(type))
                {
                    context.Errors.Add(S["Could not find type value. The index will not be imported"]);

                    continue;
                }

                if (!_indexingOptions.Sources.TryGetValue(new IndexEntityKey(providerName, type), out var _))
                {
                    context.Errors.Add(S["Unable to find a provider named '{0}' with the type '{1}'.", providerName, type]);

                    return;
                }

                index = await _indexEntityManager.NewAsync(providerName, type, token);
            }

            var validationResult = await _indexEntityManager.ValidateAsync(index);

            if (!validationResult.Succeeded)
            {
                foreach (var error in validationResult.Errors)
                {
                    context.Errors.Add(error.ErrorMessage);
                }

                continue;
            }

            await _indexEntityManager.CreateAsync(index);
        }
    }

    private sealed class IndexEntityStepModel
    {
        public JsonArray Indexes { get; set; }
    }
}
