using OrchardCore.Recipes.Schema;
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
    protected override RecipeStepSchema BuildSchema()
    {
        // Build schemas for all registered content parts.
        var partSettingsSchemas = new List<RecipeStepSchema>();
        foreach (var handler in _partSchemaHandlers)
        {
            var schema = handler.GetSettingsSchema();
            if (schema is not null)
            {
                partSettingsSchemas.Add(schema);
            }
        }

        // Build schemas for all registered content fields.
        var fieldSettingsSchemas = new List<RecipeStepSchema>();
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

        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Content Definition")
            .Description("Creates or updates content type and part definitions.")
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("ContentTypes", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Items(contentTypeSchema)
                    .Description("Content type definitions to create or update.")),
                ("ContentParts", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Items(contentPartSchema)
                    .Description("Content part definitions to create or update.")))
            .Build();
    }

    private RecipeStepSchema BuildContentTypeSchema(List<RecipeStepSchema> partSettingsSchemas)
    {
        // Build the ContentTypePartDefinitionRecords schema with known part settings.
        var partSchema = BuildContentTypePartSchema(partSettingsSchemas);

        return new RecipeStepSchemaBuilder()
            .TypeObject()
            .Required("Name")
            .Properties(
                ("Name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The technical name of the content type.")),
                ("DisplayName", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The display name of the content type.")),
                ("Settings", new RecipeStepSchemaBuilder()
                    .AnyOf(
                        // Known structure: ContentTypeSettings object.
                        new RecipeStepSchemaBuilder()
                            .TypeObject()
                            .Properties(
                                ("ContentTypeSettings", new RecipeStepSchemaBuilder()
                                    .TypeObject()
                                    .Properties(
                                        ("Creatable", new RecipeStepSchemaBuilder().TypeBoolean()),
                                        ("Listable", new RecipeStepSchemaBuilder().TypeBoolean()),
                                        ("Draftable", new RecipeStepSchemaBuilder().TypeBoolean()),
                                        ("Versionable", new RecipeStepSchemaBuilder().TypeBoolean()),
                                        ("Securable", new RecipeStepSchemaBuilder().TypeBoolean()))
                                    .AdditionalProperties(false)))
                            .AdditionalProperties(true),
                        // Fallback: any object.
                        new RecipeStepSchemaBuilder()
                            .TypeObject()
                            .AdditionalProperties(true))
                    .Description("Settings for this content type.")),
                ("ContentTypePartDefinitionRecords", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Items(partSchema)
                    .Description("Parts attached to this content type.")))
            .AdditionalProperties(true)
            .Build();
    }

    private RecipeStepSchema BuildContentTypePartSchema(List<RecipeStepSchema> partSettingsSchemas)
    {
        // Base schema for a content type part.
        var partsSchemaBuilder = new RecipeStepSchemaBuilder()
            .TypeObject()
            .Properties(
                ("PartName", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The name of the part being attached.")
                    .Enum(_contentOptions.ContentPartOptions.Select(p => p.Type.Name).ToArray())),
                ("Name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The technical name of the part attachment.")),
                ("Settings", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .AdditionalProperties(true)))
            .Required("PartName", "Name")
            .AdditionalProperties(true);

        // Merge all dynamic part settings into Settings.
        if (partSettingsSchemas.Count > 0)
        {
            partsSchemaBuilder = partsSchemaBuilder
                .Properties(
                    ("Settings", new RecipeStepSchemaBuilder()
                        .TypeObject()
                        .AnyOf(partSettingsSchemas)
                        .AdditionalProperties(true)));
        }

        return partsSchemaBuilder.Build();
    }

    private RecipeStepSchema BuildContentPartSchema(List<RecipeStepSchema> partSettingsSchemas, List<RecipeStepSchema> fieldSettingsSchemas)
    {
        var fieldSchema = BuildContentPartFieldSchema(fieldSettingsSchemas);

        var knownSettingsSchema = BuildKnownContentPartSettingsSchema();

        // Combine known + dynamic settings into ONE anyOf
        var anyOfSchemas = new List<RecipeStepSchema> { knownSettingsSchema };
        anyOfSchemas.AddRange(partSettingsSchemas);

        var settingsSchema = new RecipeStepSchemaBuilder()
            .TypeObject()
            .AnyOf(anyOfSchemas)
            .AdditionalProperties(true)
            .Build();

        var partsSchemaBuilder = new RecipeStepSchemaBuilder()
            .TypeObject()
            .Required("Name")
            .Property("Name", new RecipeStepSchemaBuilder()
                .TypeString()
                .Description("The technical name of the content part."))
            .Property("Settings", settingsSchema)
            .Property("ContentPartFieldDefinitionRecords", new RecipeStepSchemaBuilder()
                .TypeArray()
                .Items(fieldSchema)
                .Description("Fields defined on this content part."))
            .AdditionalProperties(true);

        return partsSchemaBuilder.Build();
    }

    private static RecipeStepSchema BuildKnownContentPartSettingsSchema()
    {
        return new RecipeStepSchemaBuilder()
            .TypeObject()
            .Properties(
                ("ContentPartSettings", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .Properties(
                        ("Attachable", new RecipeStepSchemaBuilder()
                            .TypeBoolean()
                            .Description("Whether this part can be manually attached to a content type.")),
                        ("Reusable", new RecipeStepSchemaBuilder()
                            .TypeBoolean()
                            .Description("Whether the part can be attached multiple times to a content type.")),
                        ("DisplayName", new RecipeStepSchemaBuilder()
                            .TypeString()
                            .Description("The displayed name of the part.")),
                        ("Description", new RecipeStepSchemaBuilder()
                            .TypeString()
                            .Description("The description of the part.")),
                        ("DefaultPosition", new RecipeStepSchemaBuilder()
                            .TypeString()
                            .Description("The default position of the part when attached to a type.")))
                    )
             )
            .AdditionalProperties(true)
            .Build();
    }


    private static RecipeStepSchema BuildKnownContentFieldSettingsSchema()
    {
        return new RecipeStepSchemaBuilder()
            .TypeObject()
            .Properties(
                ("ContentPartFieldSettings", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .Properties(
                        ("DisplayName", new RecipeStepSchemaBuilder()
                            .TypeString()
                            .Description("The displayed name of the part.")),
                        ("Description", new RecipeStepSchemaBuilder()
                            .TypeString()
                            .Description("The description of the part.")),
                        ("Editor", new RecipeStepSchemaBuilder()
                            .TypeString()
                            .Description("The editor used for this field.")),
                        ("DisplayMode", new RecipeStepSchemaBuilder()
                            .TypeString()
                            .Description("The display mode used for this field.")),
                        ("Position", new RecipeStepSchemaBuilder()
                            .TypeString()
                            .Description("The position of the field within the content part.")))
                    )
             )
            .AdditionalProperties(true)
            .Build();
    }
    private RecipeStepSchema BuildContentPartFieldSchema(List<RecipeStepSchema> fieldSettingsSchemas)
    {
        var knownSettingsSchema = BuildKnownContentFieldSettingsSchema();

        // Combine known + dynamic settings into ONE anyOf
        var anyOfSchemas = new List<RecipeStepSchema> { knownSettingsSchema };
        anyOfSchemas.AddRange(fieldSettingsSchemas);

        var settingsSchema = new RecipeStepSchemaBuilder()
            .TypeObject()
            .AnyOf(anyOfSchemas)
            .AdditionalProperties(true)
            .Build();

        // Base schema for a content part field.
        var fieldSchemaBuilder = new RecipeStepSchemaBuilder()
            .TypeObject()
            .Property("FieldName", new RecipeStepSchemaBuilder()
                .TypeString()
                .Description("The type of field.")
                .Enum(_contentOptions.ContentFieldOptions.Select(f => f.Type.Name).ToArray()))
            .Property("Name", new RecipeStepSchemaBuilder()
                .TypeString()
                .Description("The technical name of the field."))
            .Property("Settings", settingsSchema)
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
