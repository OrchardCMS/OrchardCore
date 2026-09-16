using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Utilities;
using OrchardCore.ContentTypes.Events;
using OrchardCore.ContentTypes.Management;
using OrchardCore.ContentTypes.Models;
using OrchardCore.ContentTypes.Services;
using OrchardCore.Json;
using OrchardCore.Modules;
using OrchardCore.RemoteManagement;

namespace OrchardCore.ContentTypes.Services;

internal sealed class ContentDefinitionApiService
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IContentDefinitionService _contentDefinitionService;
    private readonly IEnumerable<IContentDefinitionEventHandler> _contentDefinitionEventHandlers;
    private readonly IEnumerable<IContentDefinitionManagementSchemaProvider> _managementSchemaProviders;
    private readonly ContentOptions _contentOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly IStringLocalizer S;
    private readonly ILogger _logger;

    public ContentDefinitionApiService(
        IContentDefinitionManager contentDefinitionManager,
        IContentDefinitionService contentDefinitionService,
        IEnumerable<IContentDefinitionEventHandler> contentDefinitionEventHandlers,
        IEnumerable<IContentDefinitionManagementSchemaProvider> managementSchemaProviders,
        IOptions<ContentOptions> contentOptions,
        IOptions<DocumentJsonSerializerOptions> serializerOptions,
        IStringLocalizer<ContentDefinitionApiService> stringLocalizer,
        ILogger<ContentDefinitionApiService> logger)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _contentDefinitionService = contentDefinitionService;
        _contentDefinitionEventHandlers = contentDefinitionEventHandlers;
        _managementSchemaProviders = managementSchemaProviders;
        _contentOptions = contentOptions.Value;
        _serializerOptions = serializerOptions.Value.SerializerOptions;
        S = stringLocalizer;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ContentTypeDefinitionDto>> ListTypesAsync()
        => (await _contentDefinitionManager.ListTypeDefinitionsAsync())
            .Select(ToDto)
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public async Task<ContentTypeDefinitionDto> GetTypeAsync(string name)
    {
        var definition = await _contentDefinitionManager.LoadTypeDefinitionAsync(name);
        return definition is null ? null : ToDto(definition);
    }

    public async Task<IReadOnlyList<ContentPartDefinitionDto>> ListPartsAsync()
        => (await _contentDefinitionManager.ListPartDefinitionsAsync())
            .Select(ToDto)
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public async Task<ContentPartDefinitionDto> GetPartAsync(string name)
    {
        var definition = await _contentDefinitionManager.LoadPartDefinitionAsync(name);
        return definition is null ? null : ToDto(definition);
    }

    public async Task<IReadOnlyList<ContentPartFieldDefinitionDto>> ListFieldsAsync(string partName)
    {
        var definition = await _contentDefinitionManager.LoadPartDefinitionAsync(partName);
        return definition?.Fields
            .Select(ToDto)
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<ContentPartFieldDefinitionDto> GetFieldAsync(string partName, string fieldName)
    {
        var definition = await _contentDefinitionManager.LoadPartDefinitionAsync(partName);
        return definition?.Fields
            .Where(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase))
            .Select(ToDto)
            .FirstOrDefault();
    }

    public IReadOnlyList<ContentDefinitionSettingsSchemaDto> ListSettingsSchemas()
        => _managementSchemaProviders
            .SelectMany(provider => provider.GetSchemas())
            .GroupBy(schema => schema.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(schema => schema.Name, StringComparer.OrdinalIgnoreCase)
            .Select(ToDto)
            .ToArray();

    public ContentDefinitionSettingsSchemaDto GetSettingsSchema(string name)
        => _managementSchemaProviders
            .SelectMany(provider => provider.GetSchemas())
            .Where(schema => string.Equals(schema.Name, name, StringComparison.OrdinalIgnoreCase))
            .Select(ToDto)
            .FirstOrDefault();

    private ContentDefinitionSettingsSchemaDto ToDto(ContentDefinitionManagementSchema schema)
        => new()
        {
            Name = schema.Name,
            Scope = schema.Scope.ToString(),
            AppliesTo = schema.AppliesTo,
            Schema = ContentDefinitionSchemaBuilder.BuildSchema(schema.Type, _serializerOptions),
        };

    public async Task<ContentTypeDefinitionDto> CreateTypeAsync(ContentTypeDefinitionDto model, ModelStateDictionary modelState)
    {
        NormalizeType(model);
        await NormalizePartReferencesAsync(model);
        var existing = string.IsNullOrWhiteSpace(model.Name)
            ? null
            : await _contentDefinitionManager.LoadTypeDefinitionAsync(model.Name);
        if (existing is not null)
        {
            var existingDto = ToDto(existing);
            if (!AreEquivalent(model, existingDto))
            {
                modelState.AddModelError(nameof(ContentTypeDefinitionDto.Name), S["A content type with the same technical name already exists with a different definition."]);
            }

            return existingDto;
        }

        await ValidateTypeAsync(model, modelState);

        if (!modelState.IsValid)
        {
            return null;
        }

        await _contentDefinitionService.AddTypeAsync(model.Name, model.DisplayName);

        var definition = await BuildTypeDefinitionAsync(model, modelState);
        if (!modelState.IsValid || definition is null)
        {
            return null;
        }

        await _contentDefinitionManager.StoreTypeDefinitionAsync(definition);
        await NotifyContentTypeUpdatedAsync(definition);

        return ToDto(definition);
    }

    public async Task<ContentTypeDefinitionDto> UpdateTypeAsync(string name, ContentTypeDefinitionDto model, ModelStateDictionary modelState)
    {
        NormalizeType(model, name);

        var existing = await _contentDefinitionManager.LoadTypeDefinitionAsync(name);
        if (existing is null)
        {
            return null;
        }

        if (!string.Equals(name, model.Name, StringComparison.Ordinal))
        {
            modelState.AddModelError(nameof(ContentTypeDefinitionDto.Name), S["The content type name in the request body must match the route name. To rename a content type, use a dedicated rename operation."]);
            return ToDto(existing);
        }

        await ValidateTypeAsync(model, modelState, existingName: name);
        if (!modelState.IsValid)
        {
            return ToDto(existing);
        }

        var definition = await BuildTypeDefinitionAsync(model, modelState);
        if (!modelState.IsValid || definition is null)
        {
            return ToDto(await _contentDefinitionManager.LoadTypeDefinitionAsync(name));
        }

        await _contentDefinitionManager.StoreTypeDefinitionAsync(definition);
        await NotifyContentTypeUpdatedAsync(definition);

        return ToDto(definition);
    }

    public async Task<bool> DeleteTypeAsync(string name)
    {
        if (await _contentDefinitionManager.LoadTypeDefinitionAsync(name) is null)
        {
            return false;
        }

        await _contentDefinitionService.RemoveTypeAsync(name, false);
        return true;
    }

    public async Task<ContentPartDefinitionDto> CreatePartAsync(ContentPartDefinitionDto model, ModelStateDictionary modelState)
    {
        NormalizePart(model);
        var existing = string.IsNullOrWhiteSpace(model.Name)
            ? null
            : await _contentDefinitionManager.LoadPartDefinitionAsync(model.Name);
        if (existing is not null)
        {
            var existingDto = ToDto(existing);
            if (!AreEquivalent(model, existingDto))
            {
                modelState.AddModelError(nameof(ContentPartDefinitionDto.Name), S["A content part with the same technical name already exists with a different definition."]);
            }

            return existingDto;
        }

        await ValidatePartAsync(model, modelState);

        if (!modelState.IsValid)
        {
            return null;
        }

        await _contentDefinitionService.AddPartAsync(model.Name);

        var definition = BuildPartDefinition(model, modelState);
        if (!modelState.IsValid || definition is null)
        {
            return null;
        }

        await _contentDefinitionManager.StorePartDefinitionAsync(definition);
        await NotifyContentPartUpdatedAsync(definition);

        return ToDto(definition);
    }

    public async Task<ContentPartDefinitionDto> UpdatePartAsync(string name, ContentPartDefinitionDto model, ModelStateDictionary modelState)
    {
        NormalizePart(model, name);

        var existing = await _contentDefinitionManager.LoadPartDefinitionAsync(name);
        if (existing is null)
        {
            return null;
        }

        if (!string.Equals(name, model.Name, StringComparison.Ordinal))
        {
            modelState.AddModelError(nameof(ContentPartDefinitionDto.Name), S["The content part name in the request body must match the route name. To rename a content part, use a dedicated rename operation."]);
            return ToDto(existing);
        }

        await ValidatePartAsync(model, modelState, existingName: name);
        if (!modelState.IsValid)
        {
            return ToDto(existing);
        }

        var definition = BuildPartDefinition(model, modelState);
        if (!modelState.IsValid || definition is null)
        {
            return ToDto(await _contentDefinitionManager.LoadPartDefinitionAsync(name));
        }

        await _contentDefinitionManager.StorePartDefinitionAsync(definition);
        await NotifyContentPartUpdatedAsync(definition);

        return ToDto(definition);
    }

    public async Task<bool> DeletePartAsync(string name)
    {
        if (await _contentDefinitionManager.LoadPartDefinitionAsync(name) is null)
        {
            return false;
        }

        await _contentDefinitionService.RemovePartAsync(name);
        return true;
    }

    public async Task<ContentPartFieldDefinitionDto> CreateFieldAsync(string partName, ContentPartFieldDefinitionDto model, ModelStateDictionary modelState)
    {
        var part = await GetPartAsync(partName);
        if (part is null)
        {
            return null;
        }

        NormalizeField(model);
        var existing = part.Fields.FirstOrDefault(x => string.Equals(x.Name, model.Name, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            if (!AreEquivalent(model, existing))
            {
                modelState.AddModelError(nameof(ContentPartFieldDefinitionDto.Name), S["A content field with the same name already exists with a different definition."]);
            }

            return existing;
        }

        part.Fields.Add(model);

        var updatedPart = await UpdatePartAsync(partName, part, modelState);
        return updatedPart?.Fields.FirstOrDefault(x => string.Equals(x.Name, model.Name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<ContentPartFieldDefinitionDto> UpdateFieldAsync(string partName, string fieldName, ContentPartFieldDefinitionDto model, ModelStateDictionary modelState)
    {
        var part = await GetPartAsync(partName);
        if (part is null)
        {
            return null;
        }

        NormalizeField(model, fieldName);

        var index = part.Fields.FindIndex(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return null;
        }

        if (!string.Equals(fieldName, model.Name, StringComparison.Ordinal))
        {
            modelState.AddModelError(nameof(ContentPartFieldDefinitionDto.Name), S["The content field name in the request body must match the route name. To rename a content field, use a dedicated rename operation."]);
            return part.Fields[index];
        }

        part.Fields[index] = model;

        var updatedPart = await UpdatePartAsync(partName, part, modelState);
        return updatedPart?.Fields.FirstOrDefault(x => string.Equals(x.Name, model.Name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool?> DeleteFieldAsync(string partName, string fieldName)
    {
        var part = await GetPartAsync(partName);
        if (part is null)
        {
            return null;
        }

        var removed = part.Fields.RemoveAll(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase)) > 0;
        if (!removed)
        {
            return false;
        }

        var definition = BuildPartDefinition(part, new ModelStateDictionary());
        await _contentDefinitionManager.StorePartDefinitionAsync(definition);
        await NotifyContentPartUpdatedAsync(definition);

        return true;
    }

    public async Task<IReadOnlyList<ContentDefinitionPartTypeDescriptor>> ListPartTypesAsync()
    {
        var definitions = (await _contentDefinitionManager.ListPartDefinitionsAsync())
            .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        return _contentOptions.ContentPartOptions
            .Select(option =>
            {
                definitions.TryGetValue(option.Type.Name, out var definition);
                var settings = definition?.GetSettings<ContentPartSettings>();

                return new ContentDefinitionPartTypeDescriptor
                {
                    Name = option.Type.Name,
                    Attachable = settings?.Attachable ?? false,
                    Reusable = settings?.Reusable ?? false,
                    Schema = ContentDefinitionSchemaBuilder.BuildSchema(option.Type, _serializerOptions),
                };
            })
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public IReadOnlyList<ContentDefinitionFieldTypeDescriptor> ListFieldTypes()
        => _contentOptions.ContentFieldOptions
            .Select(option => new ContentDefinitionFieldTypeDescriptor
            {
                Name = option.Type.Name,
                Schema = ContentDefinitionSchemaBuilder.BuildSchema(option.Type, _serializerOptions),
            })
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private async Task ValidateTypeAsync(ContentTypeDefinitionDto model, ModelStateDictionary modelState, string existingName = null)
    {
        ValidateTechnicalName(modelState, nameof(ContentTypeDefinitionDto.Name), model.Name, allowReservedNames: false);

        if (string.IsNullOrWhiteSpace(model.DisplayName))
        {
            modelState.AddModelError(nameof(ContentTypeDefinitionDto.DisplayName), S["The display name is required."]);
        }

        var existingTypes = await _contentDefinitionManager.ListTypeDefinitionsAsync();
        if (existingTypes.Any(x => string.Equals(x.Name, model.Name, StringComparison.OrdinalIgnoreCase) && !string.Equals(x.Name, existingName, StringComparison.OrdinalIgnoreCase)))
        {
            modelState.AddModelError(nameof(ContentTypeDefinitionDto.Name), S["A content type with the same technical name already exists."]);
        }

        if (existingTypes.Any(x => string.Equals(x.DisplayName, model.DisplayName, StringComparison.OrdinalIgnoreCase) && !string.Equals(x.Name, existingName, StringComparison.OrdinalIgnoreCase)))
        {
            modelState.AddModelError(nameof(ContentTypeDefinitionDto.DisplayName), S["A content type with the same display name already exists."]);
        }

        var partNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < model.Parts.Count; i++)
        {
            var part = model.Parts[i];
            part.Name ??= part.PartName;
            ValidateTechnicalName(modelState, $"parts[{i}].name", part.Name, allowReservedNames: true);

            if (string.IsNullOrWhiteSpace(part.PartName))
            {
                modelState.AddModelError($"parts[{i}].partName", S["The part name is required."]);
                continue;
            }

            if (!partNames.Add(part.Name))
            {
                modelState.AddModelError($"parts[{i}].name", S["A content type cannot contain the same part more than once."]);
            }

            var definition = await _contentDefinitionManager.LoadPartDefinitionAsync(part.PartName);
            if (definition is null && !string.Equals(part.PartName, model.Name, StringComparison.OrdinalIgnoreCase))
            {
                modelState.AddModelError($"parts[{i}].partName", S["The specified content part does not exist."]);
                continue;
            }

            var partSettings = definition?.GetSettings<ContentPartSettings>();
            if (definition is not null && !string.Equals(part.PartName, model.Name, StringComparison.OrdinalIgnoreCase) && !(partSettings?.Attachable ?? false))
            {
                modelState.AddModelError($"parts[{i}].partName", S["Only attachable content parts can be added to a content type."]);
            }

            if (!string.Equals(part.Name, part.PartName, StringComparison.OrdinalIgnoreCase) && !(partSettings?.Reusable ?? false))
            {
                modelState.AddModelError($"parts[{i}].name", S["Only reusable content parts can be renamed when attached to a content type."]);
            }
        }
    }

    private async Task ValidatePartAsync(ContentPartDefinitionDto model, ModelStateDictionary modelState, string existingName = null)
    {
        ValidateTechnicalName(modelState, nameof(ContentPartDefinitionDto.Name), model.Name, allowReservedNames: true);

        var existingParts = await _contentDefinitionManager.ListPartDefinitionsAsync();
        if (existingParts.Any(x => string.Equals(x.Name, model.Name, StringComparison.OrdinalIgnoreCase) && !string.Equals(x.Name, existingName, StringComparison.OrdinalIgnoreCase)))
        {
            modelState.AddModelError(nameof(ContentPartDefinitionDto.Name), S["A content part with the same technical name already exists."]);
        }

        var existingFieldNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < model.Fields.Count; i++)
        {
            var field = model.Fields[i];
            ValidateTechnicalName(modelState, $"fields[{i}].name", field.Name, allowReservedNames: true);

            if (string.IsNullOrWhiteSpace(field.FieldName))
            {
                modelState.AddModelError($"fields[{i}].fieldName", S["The field type is required."]);
            }
            else if (_contentOptions.ContentFieldOptions.All(x => !string.Equals(x.Type.Name, field.FieldName, StringComparison.OrdinalIgnoreCase)))
            {
                modelState.AddModelError($"fields[{i}].fieldName", S["The specified content field type does not exist."]);
            }

            if (!existingFieldNames.Add(field.Name))
            {
                modelState.AddModelError($"fields[{i}].name", S["A content part cannot contain the same field more than once."]);
            }
        }
    }

    private async Task<ContentTypeDefinition> BuildTypeDefinitionAsync(ContentTypeDefinitionDto model, ModelStateDictionary modelState)
    {
        var parts = new List<ContentTypePartDefinition>(model.Parts.Count);

        foreach (var part in model.Parts)
        {
            var definition = await _contentDefinitionManager.LoadPartDefinitionAsync(part.PartName);
            if (definition is null)
            {
                modelState.AddModelError(nameof(ContentTypeDefinitionDto.Parts), S["The specified content part does not exist."]);
                return null;
            }

            parts.Add(new ContentTypePartDefinition(part.Name, definition, part.Settings.ToJsonObject()));
        }

        return new ContentTypeDefinition(model.Name, model.DisplayName, parts, model.Settings.ToJsonObject());
    }

    private ContentPartDefinition BuildPartDefinition(ContentPartDefinitionDto model, ModelStateDictionary modelState)
    {
        var fields = new List<ContentPartFieldDefinition>(model.Fields.Count);

        foreach (var field in model.Fields)
        {
            if (_contentOptions.ContentFieldOptions.All(x => !string.Equals(x.Type.Name, field.FieldName, StringComparison.OrdinalIgnoreCase)))
            {
                modelState.AddModelError(nameof(ContentPartDefinitionDto.Fields), S["The specified content field type does not exist."]);
                return null;
            }

            var fieldDefinition = new ContentFieldDefinition(field.FieldName);
            fields.Add(new ContentPartFieldDefinition(fieldDefinition, field.Name, field.Settings.ToJsonObject()));
        }

        return new ContentPartDefinition(model.Name, fields, model.Settings.ToJsonObject());
    }

    private static ContentTypeDefinitionDto ToDto(ContentTypeDefinition definition)
        => new()
        {
            Name = definition.Name,
            DisplayName = definition.DisplayName,
            Settings = ContentDefinitionSettingsDto.FromJsonObject<ContentTypeDefinitionSettingsDto>(definition.Settings),
            Parts = definition.Parts.Select(ToDto).ToList(),
        };

    private static ContentTypePartDefinitionDto ToDto(ContentTypePartDefinition definition)
        => new()
        {
            Name = definition.Name,
            PartName = definition.PartDefinition.Name,
            Settings = ContentDefinitionSettingsDto.FromJsonObject<ContentTypePartDefinitionSettingsDto>(definition.Settings),
        };

    private static ContentPartDefinitionDto ToDto(ContentPartDefinition definition)
        => new()
        {
            Name = definition.Name,
            Settings = ContentDefinitionSettingsDto.FromJsonObject<ContentPartDefinitionSettingsDto>(definition.Settings),
            Fields = definition.Fields.Select(ToDto).ToList(),
        };

    private static ContentPartFieldDefinitionDto ToDto(ContentPartFieldDefinition definition)
        => new()
        {
            Name = definition.Name,
            FieldName = definition.FieldDefinition.Name,
            Settings = ContentDefinitionSettingsDto.FromJsonObject<ContentPartFieldDefinitionSettingsDto>(definition.Settings),
        };

    private bool AreEquivalent<T>(T left, T right)
        => JsonNode.DeepEquals(
            JsonSerializer.SerializeToNode(left, _serializerOptions),
            JsonSerializer.SerializeToNode(right, _serializerOptions));

    private async Task NormalizePartReferencesAsync(ContentTypeDefinitionDto model)
    {
        var definitions = (await _contentDefinitionManager.ListPartDefinitionsAsync())
            .ToDictionary(definition => definition.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var part in model.Parts)
        {
            if (!string.IsNullOrWhiteSpace(part.PartName)
                && definitions.TryGetValue(part.PartName, out var definition))
            {
                part.PartName = definition.Name;
            }
        }
    }

    private static void NormalizeType(ContentTypeDefinitionDto model, string name = null)
    {
        model.Name = string.IsNullOrWhiteSpace(model.Name) ? name : model.Name.Trim();
        model.DisplayName = model.DisplayName?.Trim();
        model.Settings ??= new();
        model.Parts ??= [];

        foreach (var part in model.Parts)
        {
            part.Name = string.IsNullOrWhiteSpace(part.Name) ? part.PartName?.Trim() : part.Name.Trim();
            part.PartName = part.PartName?.Trim();
            part.Settings ??= new();
        }
    }

    private static void NormalizePart(ContentPartDefinitionDto model, string name = null)
    {
        model.Name = string.IsNullOrWhiteSpace(model.Name) ? name : model.Name.Trim();
        model.Settings ??= new();
        model.Fields ??= [];

        foreach (var field in model.Fields)
        {
            NormalizeField(field);
        }
    }

    private static void NormalizeField(ContentPartFieldDefinitionDto model, string name = null)
    {
        model.Name = string.IsNullOrWhiteSpace(model.Name) ? name : model.Name.Trim();
        model.FieldName = model.FieldName?.Trim();
        model.Settings ??= new();
    }

    private void ValidateTechnicalName(ModelStateDictionary modelState, string key, string value, bool allowReservedNames)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            modelState.AddModelError(key, S["The technical name is required."]);
            return;
        }

        if (!char.IsLetter(value[0]))
        {
            modelState.AddModelError(key, S["The technical name must start with a letter."]);
        }

        if (!string.Equals(value, OrchardCore.ContentManagement.Utilities.StringExtensions.ToSafeName(value), StringComparison.Ordinal))
        {
            modelState.AddModelError(key, S["The technical name contains invalid characters."]);
        }

        if (!allowReservedNames && value.IsReservedContentName())
        {
            modelState.AddModelError(key, S["The technical name is reserved."]);
        }
    }

    private Task NotifyContentTypeUpdatedAsync(ContentTypeDefinition definition)
    {
        _contentDefinitionEventHandlers.Invoke((handler, context) => handler.ContentTypeUpdated(context), new ContentTypeUpdatedContext { ContentTypeDefinition = definition }, _logger);
        return Task.CompletedTask;
    }

    private Task NotifyContentPartUpdatedAsync(ContentPartDefinition definition)
    {
        _contentDefinitionEventHandlers.Invoke((handler, context) => handler.ContentPartUpdated(context), new ContentPartUpdatedContext { ContentPartDefinition = definition }, _logger);
        return Task.CompletedTask;
    }
}

internal sealed class ContentDefinitionRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
            [
                new RemoteManagementCapability
                {
                    Id = "content-definitions",
                    Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
                    DisplayName = "Content Definitions",
                },
            ]);
}

internal static class ContentDefinitionSchemaBuilder
{
    public static JsonObject BuildSchema(Type type, JsonSerializerOptions serializerOptions)
        => serializerOptions.GetJsonSchemaAsNode(
            type,
            new JsonSchemaExporterOptions
            {
                TransformSchemaNode = static (context, node) =>
                {
                    if (context.PropertyInfo?.AttributeProvider?.GetCustomAttributes(typeof(DescriptionAttribute), true)
                        .OfType<DescriptionAttribute>()
                        .FirstOrDefault() is { } description)
                    {
                        // Boolean schemas cannot carry annotations. Preserve their validation semantics.
                        var annotated = node as JsonObject ?? new JsonObject { ["allOf"] = new JsonArray(node.DeepClone()) };
                        annotated["description"] = description.Description;
                        return annotated;
                    }

                    return node;
                },
            }) as JsonObject
            ?? [];
}
