using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OrchardCore.Json;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Recipes;

/// <summary>
/// This recipe step creates a set of sitemaps.
/// </summary>
public sealed class SitemapsStep : NamedRecipeStepHandler
{
    private readonly ISitemapManager _sitemapManager;
    private readonly DocumentJsonSerializerOptions _documentJsonSerializerOptions;

    public SitemapsStep(
        ISitemapManager sitemapManager,
        IOptions<DocumentJsonSerializerOptions> documentJsonSerializerOptions)
        : base("Sitemaps")
    {
        _sitemapManager = sitemapManager;
        _documentJsonSerializerOptions = documentJsonSerializerOptions.Value;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<SitemapStepModel>();

        foreach (var token in model.Data.Cast<JsonObject>())
        {
            var sitemap = token.ToObject<SitemapType>(_documentJsonSerializerOptions.SerializerOptions);
            await _sitemapManager.UpdateSitemapAsync(sitemap);
        }
    }

    public sealed class SitemapStepModel
    {
        public JsonArray Data { get; set; }
    }
}
