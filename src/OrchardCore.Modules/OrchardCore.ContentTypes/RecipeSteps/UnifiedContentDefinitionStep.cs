using System.Text.Json.Nodes;
using Json.Schema;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.ContentTypes.RecipeSteps;

/// <summary>
/// Unified recipe/deployment step for importing and exporting content type and part definitions.
/// </summary>
public sealed class UnifiedContentDefinitionStep : RecipeDeploymentStep<UnifiedContentDefinitionStep.ContentDefinitionStepModel>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IEnumerable<IContentDefinitionSchemaHandler> _schemaHandlers;

    internal readonly IStringLocalizer S;

    public UnifiedContentDefinitionStep(
        IContentDefinitionManager contentDefinitionManager,
        IEnumerable<IContentDefinitionSchemaHandler> schemaHandlers,
        IStringLocalizer<UnifiedContentDefinitionStep> stringLocalizer)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _schemaHandlers = schemaHandlers;
        S = stringLocalizer;
    }

    /// <inheritdoc />
    public override string Name => "ContentDefinition";

    /// <inheritdoc />
    public override string DisplayName => "Content Definition";

    /// <inheritdoc />
    public override string Description => "Creates or updates content type and part definitions.";

    /// <inheritdoc />
    public override string Category => "Content";

    /// <inheritdoc />
    protected override JsonSchema BuildSchema()
    {
        var contentTypePartSchema = new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Required("Name", "PartName")
            .Properties(
                ("Name", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The technical name of the part attachment.")),
                ("PartName", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The name of the part being attached.")),
                ("Settings", new JsonSchemaBuilder().Type(SchemaValueType.Object).AdditionalProperties(JsonSchema.Empty).Description("Settings for this part attachment.")))
            .AdditionalProperties(JsonSchema.Empty)
            .Build();

        var contentPartFieldSchema = new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Required("Name", "FieldName")
            .Properties(
                ("Name", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The technical name of the field.")),
                ("FieldName", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The type of field.")),
                ("Settings", new JsonSchemaBuilder().Type(SchemaValueType.Object).AdditionalProperties(JsonSchema.Empty).Description("Settings for this field.")))
            .AdditionalProperties(JsonSchema.Empty)
            .Build();

        var contentTypeSchema = new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Required("Name")
            .Properties(
                ("Name", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The technical name of the content type.")),
                ("DisplayName", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The display name of the content type.")),
                ("Settings", new JsonSchemaBuilder().Type(SchemaValueType.Object).AdditionalProperties(JsonSchema.Empty).Description("Settings for this content type.")),
                ("ContentTypePartDefinitionRecords", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Array)
                    .Items(contentTypePartSchema)
                    .Description("Parts attached to this content type.")))
            .AdditionalProperties(JsonSchema.Empty)
            .Build();

        var contentPartSchema = new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Required("Name")
            .Properties(
                ("Name", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The technical name of the content part.")),
                ("Settings", new JsonSchemaBuilder().Type(SchemaValueType.Object).AdditionalProperties(JsonSchema.Empty).Description("Settings for this content part.")),
                ("ContentPartFieldDefinitionRecords", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Array)
                    .Items(contentPartFieldSchema)
                    .Description("Fields defined on this content part.")))
            .AdditionalProperties(JsonSchema.Empty)
            .Build();

        var schemaBuilder = new JsonSchemaBuilder()
            .Schema(MetaSchemas.Draft202012Id)
            .Type(SchemaValueType.Object)
            .Title(Name)
            .Description(Description)
            .Required("name")
            .Properties(
                ("name", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("ContentTypes", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Array)
                    .Items(contentTypeSchema)
                    .Description("Content type definitions to create or update.")),
                ("ContentParts", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Array)
                    .Items(contentPartSchema)
                    .Description("Content part definitions to create or update.")));

        // Allow schema handlers to contribute additional schema definitions.
        var context = new ContentDefinitionSchemaContext(schemaBuilder);
        foreach (var handler in _schemaHandlers.OrderBy(h => h.Order))
        {
            handler.BuildSchema(context);
        }

        return schemaBuilder.Build();
    }

    /// <inheritdoc />
    protected override async Task ImportAsync(ContentDefinitionStepModel model, RecipeExecutionContext context)
    {
        foreach (var contentType in model.ContentTypes ?? [])
        {
            var existingType = await _contentDefinitionManager.LoadTypeDefinitionAsync(contentType.Name)
                ?? new ContentTypeDefinition(contentType.Name, contentType.DisplayName);

            await UpdateContentTypeAsync(existingType, contentType, context);
        }

        foreach (var contentPart in model.ContentParts ?? [])
        {
            var existingPart = await _contentDefinitionManager.LoadPartDefinitionAsync(contentPart.Name)
                ?? new ContentPartDefinition(contentPart.Name);

            await UpdateContentPartAsync(existingPart, contentPart, context);
        }
    }

    /// <inheritdoc />
    protected override async Task<ContentDefinitionStepModel> BuildExportModelAsync(RecipeExportContext context)
    {
        var contentTypes = await _contentDefinitionManager.ListTypeDefinitionsAsync();
        var contentParts = await _contentDefinitionManager.ListPartDefinitionsAsync();

        return new ContentDefinitionStepModel
        {
            ContentTypes = contentTypes.Select(ToRecord).ToArray(),
            ContentParts = contentParts.Select(ToRecord).ToArray(),
        };
    }

    private Task UpdateContentTypeAsync(ContentTypeDefinition type, ContentTypeDefinitionRecord record, RecipeExecutionContext context)
    {
        return _contentDefinitionManager.AlterTypeDefinitionAsync(type.Name, builder =>
        {
            if (!string.IsNullOrEmpty(record.DisplayName))
            {
                builder.WithDisplayName(record.DisplayName);
            }

            builder.MergeSettings(record.Settings);

            foreach (var part in record.ContentTypePartDefinitionRecords ?? [])
            {
                if (string.IsNullOrEmpty(part.PartName))
                {
                    context.Errors.Add(S["Unable to add content-part to the '{0}' content-type. The part name cannot be null or empty.", type.Name]);
                    continue;
                }

                builder.WithPart(part.Name, part.PartName, partBuilder => partBuilder.MergeSettings(part.Settings));
            }
        });
    }

    private Task UpdateContentPartAsync(ContentPartDefinition part, ContentPartDefinitionRecord record, RecipeExecutionContext context)
    {
        return _contentDefinitionManager.AlterPartDefinitionAsync(part.Name, builder =>
        {
            builder.MergeSettings(record.Settings);

            foreach (var field in record.ContentPartFieldDefinitionRecords ?? [])
            {
                if (string.IsNullOrEmpty(field.Name))
                {
                    context.Errors.Add(S["Unable to add content-field to the '{0}' content-part. The field name cannot be null or empty.", part.Name]);
                    continue;
                }

                builder.WithField(field.Name, fieldBuilder =>
                {
                    fieldBuilder.OfType(field.FieldName);
                    fieldBuilder.MergeSettings(field.Settings);
                });
            }
        });
    }

    private static ContentTypeDefinitionRecord ToRecord(ContentTypeDefinition type)
    {
        return new ContentTypeDefinitionRecord
        {
            Name = type.Name,
            DisplayName = type.DisplayName,
            Settings = type.Settings,
            ContentTypePartDefinitionRecords = type.Parts.Select(p => new ContentTypePartDefinitionRecord
            {
                Name = p.Name,
                PartName = p.PartDefinition.Name,
                Settings = p.Settings,
            }).ToArray(),
        };
    }

    private static ContentPartDefinitionRecord ToRecord(ContentPartDefinition part)
    {
        return new ContentPartDefinitionRecord
        {
            Name = part.Name,
            Settings = part.Settings,
            ContentPartFieldDefinitionRecords = part.Fields.Select(f => new ContentPartFieldDefinitionRecord
            {
                Name = f.Name,
                FieldName = f.FieldDefinition.Name,
                Settings = f.Settings,
            }).ToArray(),
        };
    }

    /// <summary>
    /// Model for the ContentDefinition step data.
    /// </summary>
    public sealed class ContentDefinitionStepModel
    {
        public string Name { get; set; }
        public ContentTypeDefinitionRecord[] ContentTypes { get; set; } = [];
        public ContentPartDefinitionRecord[] ContentParts { get; set; } = [];
    }
}

/// <summary>
/// Handler for contributing schema definitions for content parts and fields.
/// </summary>
public interface IContentDefinitionSchemaHandler
{
    /// <summary>
    /// Gets the order in which this handler runs.
    /// </summary>
    int Order => 0;

    /// <summary>
    /// Contributes schema definitions for content parts and fields.
    /// </summary>
    void BuildSchema(ContentDefinitionSchemaContext context);
}

/// <summary>
/// Context for building content definition schemas.
/// </summary>
public sealed class ContentDefinitionSchemaContext
{
    private readonly JsonSchemaBuilder _schemaBuilder;
    private readonly Dictionary<string, JsonSchema> _definitions = [];

    public ContentDefinitionSchemaContext(JsonSchemaBuilder schemaBuilder)
    {
        _schemaBuilder = schemaBuilder;
    }

    /// <summary>
    /// Adds a schema definition for a content part's settings.
    /// </summary>
    public void AddPartSettingsSchema(string partName, JsonSchema settingsSchema)
    {
        ArgumentException.ThrowIfNullOrEmpty(partName);
        _definitions[$"Part_{partName}_Settings"] = settingsSchema;
    }

    /// <summary>
    /// Adds a schema definition for a content field's settings.
    /// </summary>
    public void AddFieldSettingsSchema(string fieldName, JsonSchema settingsSchema)
    {
        ArgumentException.ThrowIfNullOrEmpty(fieldName);
        _definitions[$"Field_{fieldName}_Settings"] = settingsSchema;
    }

    /// <summary>
    /// Adds a schema definition for content type settings.
    /// </summary>
    public void AddContentTypeSettingsSchema(string settingsName, JsonSchema settingsSchema)
    {
        ArgumentException.ThrowIfNullOrEmpty(settingsName);
        _definitions[$"TypeSettings_{settingsName}"] = settingsSchema;
    }

    /// <summary>
    /// Gets all registered definitions.
    /// </summary>
    public IReadOnlyDictionary<string, JsonSchema> Definitions => _definitions;
}
