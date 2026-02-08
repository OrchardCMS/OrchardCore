using OrchardCore.Recipes.Schema;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Search.Lucene.Recipes;

public sealed class LuceneIndexRebuildRecipeStep : RecipeImportStep<LuceneIndexRebuildRecipeStep.LuceneIndexRebuildStepModel>
{
    private readonly IIndexProfileManager _indexProfileManager;
    private readonly IServiceProvider _serviceProvider;

    public LuceneIndexRebuildRecipeStep(
        IIndexProfileManager indexProfileManager,
        IServiceProvider serviceProvider)
    {
        _indexProfileManager = indexProfileManager;
        _serviceProvider = serviceProvider;
    }

    public override string Name => "lucene-index-rebuild";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Lucene Index Rebuild")
            .Description("Rebuilds Lucene indexes.")
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("IncludeAll", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("Indices", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Items(new RecipeStepSchemaBuilder().TypeString())))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(LuceneIndexRebuildStepModel model, RecipeExecutionContext context)
    {
        if (model.IncludeAll || model.Indices.Length > 0)
        {
            var indexes = model.IncludeAll
                ? (await _indexProfileManager.GetByProviderAsync(LuceneConstants.ProviderName))
                : (await _indexProfileManager.GetByProviderAsync(LuceneConstants.ProviderName)).Where(x => model.Indices.Contains(x.IndexName));

            var indexManagers = new Dictionary<string, IIndexManager>();

            foreach (var index in indexes)
            {
                if (!indexManagers.TryGetValue(index.ProviderName, out var indexManager))
                {
                    indexManager = _serviceProvider.GetKeyedService<IIndexManager>(index.ProviderName);
                    indexManagers[index.ProviderName] = indexManager;
                }

                if (indexManager is null)
                {
                    continue;
                }

                await _indexProfileManager.ResetAsync(index);
                await _indexProfileManager.UpdateAsync(index);

                if (!await indexManager.ExistsAsync(index.IndexFullName))
                {
                    await indexManager.CreateAsync(index);
                }
                else
                {
                    await indexManager.RebuildAsync(index);
                }

                await _indexProfileManager.SynchronizeAsync(index);
            }
        }
    }

    public sealed class LuceneIndexRebuildStepModel
    {
        public bool IncludeAll { get; set; }
        public string[] Indices { get; set; } = [];
    }
}
