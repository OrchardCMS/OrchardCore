using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Indexing.Models;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Indexing.Core.Recipes;

public sealed class CreateOrUpdateIndexProfileStep : NamedRecipeStepHandler
{
    public const string StepKey = "CreateOrUpdateIndexProfile";

    private readonly IIndexProfileManager _indexProfileManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly IndexingOptions _indexingOptions;

    internal readonly IStringLocalizer S;

    public CreateOrUpdateIndexProfileStep(
        IIndexProfileManager indexProfileManager,
        IOptions<IndexingOptions> indexingOptions,
        IServiceProvider serviceProvider,
        IStringLocalizer<CreateOrUpdateIndexProfileStep> stringLocalizer)
        : base(StepKey)
    {
        _indexProfileManager = indexProfileManager;
        _serviceProvider = serviceProvider;
        _indexingOptions = indexingOptions.Value;
        S = stringLocalizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<IndexProfileStepModel>();
        var tokens = model.Indexes.Cast<JsonObject>() ?? [];

        foreach (var token in tokens)
        {
            IndexProfile indexProfile = null;

            var id = token[nameof(indexProfile.Id)]?.GetValue<string>();

            if (!string.IsNullOrEmpty(id))
            {
                indexProfile = await _indexProfileManager.FindByIdAsync(id);
            }

            if (indexProfile is null)
            {
                var name = token[nameof(indexProfile.Name)]?.GetValue<string>();

                if (!string.IsNullOrEmpty(name))
                {
                    indexProfile = await _indexProfileManager.FindByNameAsync(name);
                }
            }

            if (indexProfile is not null)
            {
                var validationResult = await _indexProfileManager.ValidateAsync(indexProfile);

                if (!validationResult.Succeeded)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        context.Errors.Add(error.ErrorMessage);
                    }

                    continue;
                }

                await _indexProfileManager.UpdateAsync(indexProfile, token);
            }
            else
            {
                var providerName = token[nameof(indexProfile.ProviderName)]?.GetValue<string>();

                if (string.IsNullOrEmpty(providerName))
                {
                    context.Errors.Add(S["Could not find provider value. The index will not be imported"]);

                    continue;
                }

                var type = token[nameof(indexProfile.Type)]?.GetValue<string>();

                if (string.IsNullOrEmpty(type))
                {
                    context.Errors.Add(S["Could not find type value. The index will not be imported"]);

                    continue;
                }

                if (!_indexingOptions.Sources.TryGetValue(new IndexProfileKey(providerName, type), out var _))
                {
                    context.Errors.Add(S["Unable to find a provider named '{0}' with the type '{1}'.", providerName, type]);

                    return;
                }

                indexProfile = await _indexProfileManager.NewAsync(providerName, type, token);

                var validationResult = await _indexProfileManager.ValidateAsync(indexProfile);

                if (!validationResult.Succeeded)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        context.Errors.Add(error.ErrorMessage);
                    }

                    continue;
                }

                var indexManager = _serviceProvider.GetRequiredKeyedService<IIndexManager>(providerName);

                await _indexProfileManager.CreateAsync(indexProfile);

                if (!await indexManager.CreateAsync(indexProfile))
                {
                    await _indexProfileManager.DeleteAsync(indexProfile);

                    context.Errors.Add(S["Unable to create the index '{0}' for the provider '{1}'.", indexProfile.IndexName, providerName]);

                    return;
                }

                await _indexProfileManager.SynchronizeAsync(indexProfile);
            }
        }
    }

    private sealed class IndexProfileStepModel
    {
        public JsonArray Indexes { get; set; }
    }
}
