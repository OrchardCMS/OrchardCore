using System.Text.Json.Nodes;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.Lucene.Model;

namespace OrchardCore.Search.Lucene.Recipes;

/// <summary>
/// This recipe step creates a Lucene index.
/// </summary>
public sealed class LuceneIndexStep : NamedRecipeStepHandler
{
    private readonly LuceneIndexingService _luceneIndexingService;
    private readonly LuceneIndexManager _luceneIndexManager;

    public LuceneIndexStep(
        LuceneIndexingService luceneIndexingService,
        LuceneIndexManager luceneIndexManager
        )
        : base("lucene-index")
    {
        _luceneIndexManager = luceneIndexManager;
        _luceneIndexingService = luceneIndexingService;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var settings = context.GetIndexSettings<LuceneIndexSettings>();

        foreach (var setting in settings)
        {
            if (!_luceneIndexManager.Exists(setting.IndexName))
            {
                await _luceneIndexingService.CreateIndexAsync(setting);
            }
        }
    }
}

public sealed class ContentStepModel
{
    public JsonObject Data { get; set; }
}
