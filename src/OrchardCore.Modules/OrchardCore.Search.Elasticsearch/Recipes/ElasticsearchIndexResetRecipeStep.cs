using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Schema;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Search.Elasticsearch.Recipes;

public sealed class ElasticsearchIndexResetRecipeStep : RecipeImportStep<ElasticsearchIndexResetRecipeStep.ElasticsearchIndexResetStepModel>
{
    private readonly IIndexProfileManager _indexProfileManager;
    private readonly IServiceProvider _serviceProvider;

    public ElasticsearchIndexResetRecipeStep(
        IIndexProfileManager indexProfileManager,
        IServiceProvider serviceProvider)
    {
        _indexProfileManager = indexProfileManager;
        _serviceProvider = serviceProvider;
    }

    public override string Name => "elastic-index-reset";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Elasticsearch Index Reset")
            .Description("Resets Elasticsearch indexes.")
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

    protected override async Task ImportAsync(ElasticsearchIndexResetStepModel model, RecipeExecutionContext context)
    {
        if (model != null && (model.IncludeAll || model.Indices.Length > 0))
        {
            var indexes = model.IncludeAll
                ? (await _indexProfileManager.GetByProviderAsync(ElasticsearchConstants.ProviderName))
                : (await _indexProfileManager.GetByProviderAsync(ElasticsearchConstants.ProviderName)).Where(x => model.Indices.Contains(x.IndexName));

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

                await _indexProfileManager.SynchronizeAsync(index);
            }
        }
    }

    public sealed class ElasticsearchIndexResetStepModel
    {
        public bool IncludeAll { get; set; }
        public string[] Indices { get; set; } = [];
    }
}
