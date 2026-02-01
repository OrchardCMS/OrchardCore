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
    protected override JsonSchema BuildSchema()
    {
        // Build schemas for all registered content parts.
        var partSettingsSchemas = new List<JsonSchema>();
        foreach (var handler in _partSchemaHandlers)
        {
            var schema = handler.GetSettingsSchema();
            if (schema is not null)
            {
                partSettingsSchemas.Add(schema);
            }
        }

        // Build schemas for all registered content fields.
        var fieldSettingsSchemas = new List<JsonSchema>();
        foreach (var handler in _fieldSchemaHandlers)
        {
            var schema = handler.GetSettingsSchema();
            if (schema is not null)
            {
                fieldSettingsSchemas.Add(schema);
            }
        }

        // Build content type schema.
        var contentTypeSchema = BuildContentTypeSchema(partSettingsSchemas);

        // Build content part schema with fields.
        var contentPartSchema = BuildContentPartSchema(partSettingsSchemas, fieldSettingsSchemas);

        return new JsonSchemaBuilder()
            .Schema(MetaSchemas.Draft202012Id)
            .Type(SchemaValueType.Object)
            .Title("Content Definition")
            .Description("Creates or updates content type and part definitions.")
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
                    .Description("Content part definitions to create or update.")))
            .Build();
    }

    private JsonSchema BuildContentTypeSchema(List<JsonSchema> partSettingsSchemas)
    {
        // Build the ContentTypePartDefinitionRecords schema with known part settings.
        var partSchema = BuildContentTypePartSchema(partSettingsSchemas);

        return new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Required("Name")
            .Properties(
                ("Name", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("The technical name of the content type.")),
                ("DisplayName", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("The display name of the content type.")),
                ("Settings", new JsonSchemaBuilder()
                    .AnyOf(
                        // Known structure: ContentTypeSettings object.
                        new JsonSchemaBuilder()
                            .Type(SchemaValueType.Object)
                            .Properties(
                                ("ContentTypeSettings", new JsonSchemaBuilder()
                                    .Type(SchemaValueType.Object)
                                    .Properties(
                                        ("Creatable", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                                        ("Listable", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                                        ("Draftable", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                                        ("Versionable", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                                        ("Securable", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)))
                                    .AdditionalProperties(false)))
                            .AdditionalProperties(true),
                        // Fallback: any object.
                        new JsonSchemaBuilder()
                            .Type(SchemaValueType.Object)
                            .AdditionalProperties(true))
                    .Description("Settings for this content type.")),
                ("ContentTypePartDefinitionRecords", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Array)
                    .Items(partSchema)
                    .Description("Parts attached to this content type.")))
            .AdditionalProperties(true)
            .Build();
    }

    private JsonSchema BuildContentTypePartSchema(List<JsonSchema> partSettingsSchemas)
    {
        // Base schema for a content type part.
        var partsSchemaBuilder = new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Properties(
                ("PartName", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("The name of the part being attached.")
                    .Enum(_contentOptions.ContentPartOptions.Select(p => p.Type.Name).ToArray())),
                ("Name", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("The technical name of the part attachment.")),
                ("Settings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .AdditionalProperties(true)))
            .Required("PartName", "Name")
            .AdditionalProperties(true);

        // Merge all dynamic part settings into Settings.
        if (partSettingsSchemas.Count > 0)
        {
            partsSchemaBuilder = partsSchemaBuilder
                .Properties(
                    ("Settings", new JsonSchemaBuilder()
                        .Type(SchemaValueType.Object)
                        .AnyOf(partSettingsSchemas)
                        .AdditionalProperties(true)));
        }

        return partsSchemaBuilder.Build();
    }

    private JsonSchema BuildContentPartSchema(List<JsonSchema> partSettingsSchemas, List<JsonSchema> fieldSettingsSchemas)
    {
        var fieldSchema = BuildContentPartFieldSchema(fieldSettingsSchemas);

        var knownSettingsSchema = BuildKnownContentPartSettingsSchema();

        // Combine known + dynamic settings into ONE anyOf
        var anyOfSchemas = new List<JsonSchema> { knownSettingsSchema };
        anyOfSchemas.AddRange(partSettingsSchemas);

        var settingsSchema = new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .AnyOf(anyOfSchemas)
            .AdditionalProperties(true)
            .Build();

        var partsSchemaBuilder = new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Required("Name")
            .Properties(
                ("Name", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("The technical name of the content part.")),
                ("Settings", settingsSchema),
                ("ContentPartFieldDefinitionRecords", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Array)
                    .Items(fieldSchema)
                    .Description("Fields defined on this content part.")))
            .AdditionalProperties(true);

        return partsSchemaBuilder.Build();
    }

    private static JsonSchema BuildKnownContentPartSettingsSchema()
    {
        return new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Properties(
                ("ContentPartSettings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .Properties(
                        ("Attachable", new JsonSchemaBuilder()
                            .Type(SchemaValueType.Boolean)
                            .Description("Whether this part can be manually attached to a content type.")),
                        ("Reusable", new JsonSchemaBuilder()
                            .Type(SchemaValueType.Boolean)
                            .Description("Whether the part can be attached multiple times to a content type.")),
                        ("DisplayName", new JsonSchemaBuilder()
                            .Type(SchemaValueType.String)
                            .Description("The displayed name of the part.")),
                        ("Description", new JsonSchemaBuilder()
                            .Type(SchemaValueType.String)
                            .Description("The description of the part.")),
                        ("DefaultPosition", new JsonSchemaBuilder()
                            .Type(SchemaValueType.String)
                            .Description("The default position of the part when attached to a type.")))
                    )
             )
            .AdditionalProperties(true)
            .Build();
    }


    private static JsonSchema BuildKnownContentFieldSettingsSchema()
    {
        return new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Properties(
                ("ContentPartFieldSettings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .Properties(
                        ("DisplayName", new JsonSchemaBuilder()
                            .Type(SchemaValueType.String)
                            .Description("The displayed name of the part.")),
                        ("Description", new JsonSchemaBuilder()
                            .Type(SchemaValueType.String)
                            .Description("The description of the part.")),
                        ("Editor", new JsonSchemaBuilder()
                            .Type(SchemaValueType.String)
                            .Description("The editor used for this field.")),
                        ("DisplayMode", new JsonSchemaBuilder()
                            .Type(SchemaValueType.String)
                            .Description("The display mode used for this field.")),
                        ("Position", new JsonSchemaBuilder()
                            .Type(SchemaValueType.String)
                            .Description("The position of the field within the content part.")))
                    )
             )
            .AdditionalProperties(true)
            .Build();
    }
    private JsonSchema BuildContentPartFieldSchema(List<JsonSchema> fieldSettingsSchemas)
    {
        var knownSettingsSchema = BuildKnownContentFieldSettingsSchema();

        // Combine known + dynamic settings into ONE anyOf
        var anyOfSchemas = new List<JsonSchema> { knownSettingsSchema };
        anyOfSchemas.AddRange(fieldSettingsSchemas);

        var settingsSchema = new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .AnyOf(anyOfSchemas)
            .AdditionalProperties(true)
            .Build();

        // Base schema for a content part field.
        var fieldSchemaBuilder = new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Properties(
                ("FieldName", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("The type of field.")
                    .Enum(_contentOptions.ContentFieldOptions.Select(f => f.Type.Name).ToArray())),
                ("Name", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("The technical name of the field.")),
                ("Settings", settingsSchema))
            .Required("FieldName", "Name")
            .AdditionalProperties(true);

        return fieldSchemaBuilder.Build();
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
