using System.Text.Json.Nodes;
using OrchardCore.Recipes.Schema;
using Microsoft.Extensions.Options;
using OrchardCore.Json;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Recipes;

public sealed class SitemapsRecipeStep : RecipeImportStep<SitemapsRecipeStep.SitemapStepModel>
{
    private readonly ISitemapManager _sitemapManager;
    private readonly DocumentJsonSerializerOptions _documentJsonSerializerOptions;

    public SitemapsRecipeStep(
        ISitemapManager sitemapManager,
        IOptions<DocumentJsonSerializerOptions> documentJsonSerializerOptions)
    {
        _sitemapManager = sitemapManager;
        _documentJsonSerializerOptions = documentJsonSerializerOptions.Value;
    }

    public override string Name => "Sitemaps";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Sitemaps")
            .Description("Creates or updates sitemaps.")
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("data", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Items(new RecipeStepSchemaBuilder()
                        .TypeObject()
                        .AdditionalProperties(true))))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(SitemapStepModel model, RecipeExecutionContext context)
    {
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
