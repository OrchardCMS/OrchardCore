using System.Text.Json.Nodes;
using Json.Schema;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
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
    public override string DisplayName => "Content";

    /// <inheritdoc />
    public override string Description => "Imports content items into the Orchard Core application.";

    /// <inheritdoc />
    public override string Category => "Content";

    /// <inheritdoc />
    protected override JsonSchema BuildSchema()
    {
        var contentItemSchema = new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Required("ContentItemId", "ContentType")
            .Properties(
                ("ContentItemId", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The unique identifier for the content item.")),
                ("ContentItemVersionId", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The version identifier for the content item.")),
                ("ContentType", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The type of content item.")),
                ("DisplayText", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The display text for the content item.")),
                ("Latest", new JsonSchemaBuilder().Type(SchemaValueType.Boolean).Description("Whether this is the latest version.")),
                ("Published", new JsonSchemaBuilder().Type(SchemaValueType.Boolean).Description("Whether this content item is published.")),
                ("ModifiedUtc", new JsonSchemaBuilder().Type(SchemaValueType.String).Format(Formats.DateTime).Description("The UTC date/time when the content item was last modified.")),
                ("PublishedUtc", new JsonSchemaBuilder().Type(SchemaValueType.String).Format(Formats.DateTime).Description("The UTC date/time when the content item was published.")),
                ("CreatedUtc", new JsonSchemaBuilder().Type(SchemaValueType.String).Format(Formats.DateTime).Description("The UTC date/time when the content item was created.")),
                ("Owner", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The owner of the content item.")),
                ("Author", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The author of the content item.")))
            .AdditionalProperties(JsonSchema.Empty)
            .Build();

        return new JsonSchemaBuilder()
            .Schema(MetaSchemas.Draft202012Id)
            .Type(SchemaValueType.Object)
            .Title(Name)
            .Description(Description)
            .Required("name", "data")
            .Properties(
                ("name", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("data", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Array)
                    .Description("Array of content items to import.")
                    .Items(contentItemSchema)))
            .AdditionalProperties(JsonSchema.False)
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
