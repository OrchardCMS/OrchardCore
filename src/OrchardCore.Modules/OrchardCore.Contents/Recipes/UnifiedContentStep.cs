using System.Text.Json.Nodes;
using OrchardCore.Recipes.Schema;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Core;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Contents.Recipes;

/// <summary>
/// Unified recipe/deployment step for importing and exporting content items.
/// </summary>
public sealed class UnifiedContentStep : RecipeDeploymentStep<UnifiedContentStep.ContentStepModel>
{
    private readonly IContentManager _contentManager;

    public UnifiedContentStep(IContentManager contentManager)
    {
        _contentManager = contentManager;
    }

    /// <inheritdoc />
    public override string Name => "Content";

    /// <inheritdoc />
    protected override JsonSchema BuildSchema()
    {


        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Content")
            .Description("Imports content items into the Orchard Core application.")
            .Required("name", "data")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("data", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Description("Array of content items to import.")
                    .Items(ContentCommonSchemas.ContentItemSchema)))
            .AdditionalProperties(true)
            .Build();
    }

    /// <inheritdoc />
    protected override Task ImportAsync(ContentStepModel model, RecipeExecutionContext context)
    {
        var contentItems = model.Data?.ToObject<ContentItem[]>() ?? [];

        // If the shell is activated there is no migration in progress.
        if (ShellScope.Context.IsActivated)
        {
            return _contentManager.ImportAsync(contentItems);
        }

        // Otherwise, the import of content items is deferred after all migrations are completed,
        // this prevents e.g. a content handler to trigger a workflow before workflows migrations.
        ShellScope.AddDeferredTask(scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            return contentManager.ImportAsync(contentItems);
        });

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override async Task<ContentStepModel> BuildExportModelAsync(RecipeExportContext context)
    {
        // Check if we have specific content item IDs to export from properties.
        if (!context.Properties.TryGetValue("ContentItemIds", out var idsObj) ||
            idsObj is not IEnumerable<string> contentItemIds)
        {
            return null;
        }

        var contentItems = new List<ContentItem>();
        foreach (var id in contentItemIds)
        {
            var contentItem = await _contentManager.GetAsync(id);
            if (contentItem is not null)
            {
                contentItems.Add(contentItem);
            }
        }

        if (contentItems.Count == 0)
        {
            return null;
        }

        return new ContentStepModel
        {
            Data = JArray.FromObject(contentItems),
        };
    }

    /// <summary>
    /// Model for the Content step data.
    /// </summary>
    public sealed class ContentStepModel
    {
        /// <summary>
        /// Gets or sets the step name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the array of content items to import.
        /// </summary>
        public JsonArray Data { get; set; }
    }
}
