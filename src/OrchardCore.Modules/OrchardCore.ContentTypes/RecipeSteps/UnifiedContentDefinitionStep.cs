using Json.Schema;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
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
    private readonly IEnumerable<IContentPartSchemaHandler> _partSchemaHandlers;
    private readonly IEnumerable<IContentFieldSchemaHandler> _fieldSchemaHandlers;
    private readonly ContentOptions _contentOptions;

    internal readonly IStringLocalizer S;

    public UnifiedContentDefinitionStep(
        IContentDefinitionManager contentDefinitionManager,
        IEnumerable<IContentPartSchemaHandler> partSchemaHandlers,
        IEnumerable<IContentFieldSchemaHandler> fieldSchemaHandlers,
        IOptions<ContentOptions> contentOptions,
        IStringLocalizer<UnifiedContentDefinitionStep> stringLocalizer)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _partSchemaHandlers = partSchemaHandlers;
        _fieldSchemaHandlers = fieldSchemaHandlers;
        _contentOptions = contentOptions.Value;
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
        var definitions = new Dictionary<string, JsonSchema>();

        // Build schemas for all registered content parts.
        var partSettingsSchemas = new Dictionary<string, JsonSchema>();
        foreach (var handler in _partSchemaHandlers.OrderBy(h => h.Order))
        {
            var schema = handler.BuildSettingsSchema();
            if (schema is not null)
            {
                partSettingsSchemas[handler.PartName] = schema;
                definitions[$"Part_{handler.PartName}_Settings"] = schema;
            }
        }

        // Build schemas for all registered content fields.
        var fieldSettingsSchemas = new Dictionary<string, JsonSchema>();
        foreach (var handler in _fieldSchemaHandlers.OrderBy(h => h.Order))
        {
            var schema = handler.BuildSettingsSchema();
            if (schema is not null)
            {
                fieldSettingsSchemas[handler.FieldName] = schema;
                definitions[$"Field_{handler.FieldName}_Settings"] = schema;
            }
        }

        // Build content type part attachment schema with dynamic settings based on part name.
        var contentTypePartSchema = BuildContentTypePartSchema(partSettingsSchemas);

        // Build content part field schema with dynamic settings based on field type.
        var contentPartFieldSchema = BuildContentPartFieldSchema(fieldSettingsSchemas);

        // Build content type schema.
        var contentTypeSchema = BuildContentTypeSchema(contentTypePartSchema);

        // Build content part schema with fields.
        var contentPartSchema = BuildContentPartSchema(contentPartFieldSchema, partSettingsSchemas);

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

        // Add all definitions to the schema.
        if (definitions.Count > 0)
        {
            schemaBuilder.Defs(definitions);
        }

        return schemaBuilder.Build();
    }

    private JsonSchema BuildContentTypePartSchema(Dictionary<string, JsonSchema> partSettingsSchemas)
    {
        // Build a settings schema that includes all known part settings as optional properties.
        var settingsProperties = new Dictionary<string, JsonSchema>();
        foreach (var (partName, schema) in partSettingsSchemas)
        {
            settingsProperties[$"{partName}Settings"] = schema;
        }

        var settingsSchema = settingsProperties.Count > 0
            ? new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(settingsProperties.Select(kvp => (kvp.Key, kvp.Value)).ToArray())
                .AdditionalProperties(JsonSchema.Empty)
                .Description("Settings for this part attachment.")
                .Build()
            : new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .AdditionalProperties(JsonSchema.Empty)
                .Description("Settings for this part attachment.")
                .Build();

        return new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Required("Name", "PartName")
            .Properties(
                ("Name", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The technical name of the part attachment.")),
                ("PartName", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("The name of the part being attached.")
                    .Enum(_contentOptions.ContentPartOptions.Select(p => p.Type.Name).ToArray())),
                ("Settings", settingsSchema))
            .AdditionalProperties(JsonSchema.Empty)
            .Build();
    }

    private JsonSchema BuildContentPartFieldSchema(Dictionary<string, JsonSchema> fieldSettingsSchemas)
    {
        // Build a settings schema that includes all known field settings as optional properties.
        var settingsProperties = new Dictionary<string, JsonSchema>();
        foreach (var (fieldName, schema) in fieldSettingsSchemas)
        {
            settingsProperties[$"{fieldName}Settings"] = schema;
        }

        var settingsSchema = settingsProperties.Count > 0
            ? new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(settingsProperties.Select(kvp => (kvp.Key, kvp.Value)).ToArray())
                .AdditionalProperties(JsonSchema.Empty)
                .Description("Settings for this field.")
                .Build()
            : new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .AdditionalProperties(JsonSchema.Empty)
                .Description("Settings for this field.")
                .Build();

        return new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Required("Name", "FieldName")
            .Properties(
                ("Name", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The technical name of the field.")),
                ("FieldName", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("The type of field.")
                    .Enum(_contentOptions.ContentFieldOptions.Select(f => f.Type.Name).ToArray())),
                ("Settings", settingsSchema))
            .AdditionalProperties(JsonSchema.Empty)
            .Build();
    }

    private static JsonSchema BuildContentTypeSchema(JsonSchema contentTypePartSchema)
    {
        return new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Required("Name")
            .Properties(
                ("Name", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The technical name of the content type.")),
                ("DisplayName", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The display name of the content type.")),
                ("Settings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .AdditionalProperties(JsonSchema.Empty)
                    .Description("Settings for this content type.")),
                ("ContentTypePartDefinitionRecords", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Array)
                    .Items(contentTypePartSchema)
                    .Description("Parts attached to this content type.")))
            .AdditionalProperties(JsonSchema.Empty)
            .Build();
    }

    private JsonSchema BuildContentPartSchema(JsonSchema contentPartFieldSchema, Dictionary<string, JsonSchema> partSettingsSchemas)
    {
        // Build settings schema with all part settings handlers.
        var settingsProperties = new Dictionary<string, JsonSchema>();
        foreach (var (partName, schema) in partSettingsSchemas)
        {
            settingsProperties[$"{partName}Settings"] = schema;
        }

        var settingsSchema = settingsProperties.Count > 0
            ? new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(settingsProperties.Select(kvp => (kvp.Key, kvp.Value)).ToArray())
                .AdditionalProperties(JsonSchema.Empty)
                .Description("Settings for this content part.")
                .Build()
            : new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .AdditionalProperties(JsonSchema.Empty)
                .Description("Settings for this content part.")
                .Build();

        return new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Required("Name")
            .Properties(
                ("Name", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("The technical name of the content part.")
                    .Enum(_contentOptions.ContentPartOptions.Select(p => p.Type.Name).ToArray())),
                ("Settings", settingsSchema),
                ("ContentPartFieldDefinitionRecords", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Array)
                    .Items(contentPartFieldSchema)
                    .Description("Fields defined on this content part.")))
            .AdditionalProperties(JsonSchema.Empty)
            .Build();
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
