using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.ViewModels;

namespace OrchardCore.ContentTypes.Services;

public interface IContentDefinitionService
{
    Task<IEnumerable<EditTypeViewModel>> LoadTypesAsync();

    Task<IEnumerable<EditTypeViewModel>> GetTypesAsync();

    Task<EditTypeViewModel> LoadTypeAsync(string name);

    Task<EditTypeViewModel> GetTypeAsync(string name);

    Task<ContentTypeDefinition> AddTypeAsync(string name, string displayName);

    Task RemoveTypeAsync(string name, bool deleteContent);

    Task AddPartToTypeAsync(string partName, string typeName);

    Task AddReusablePartToTypeAsync(string name, string displayName, string description, string partName, string typeName);

    Task RemovePartFromTypeAsync(string partName, string typeName);

    Task<string> GenerateContentTypeNameFromDisplayNameAsync(string displayName);

    Task<string> GenerateFieldNameFromDisplayNameAsync(string partName, string displayName);

    Task<IEnumerable<EditPartViewModel>> LoadPartsAsync(bool metadataPartsOnly);

    Task<IEnumerable<EditPartViewModel>> GetPartsAsync(bool metadataPartsOnly);

    Task<EditPartViewModel> LoadPartAsync(string name);

    Task<EditPartViewModel> GetPartAsync(string name);

    Task<EditPartViewModel> AddPartAsync(CreatePartViewModel partViewModel);

    Task RemovePartAsync(string name);

    Task<IEnumerable<Type>> GetFieldsAsync();

    Task AddFieldToPartAsync(string fieldName, string fieldTypeName, string partName);

    Task AddFieldToPartAsync(string fieldName, string displayName, string fieldTypeName, string partName);

    Task RemoveFieldFromPartAsync(string fieldName, string partName);

    Task AlterFieldAsync(EditPartViewModel partViewModel, EditFieldViewModel fieldViewModel);

    Task AlterTypePartAsync(EditTypePartViewModel partViewModel);

    Task AlterTypePartsOrderAsync(ContentTypeDefinition typeDefinition, string[] partNames);

    Task AlterPartFieldsOrderAsync(ContentPartDefinition partDefinition, string[] fieldNames);

    [Obsolete($"Instead, utilize the {nameof(LoadTypesAsync)} method. This current method is slated for removal in upcoming releases.")]
    IEnumerable<EditTypeViewModel> LoadTypes()
=> LoadTypesAsync().GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(GetTypesAsync)} method. This current method is slated for removal in upcoming releases.")]
    IEnumerable<EditTypeViewModel> GetTypes()
        => GetTypesAsync().GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(LoadTypeAsync)} method. This current method is slated for removal in upcoming releases.")]
    EditTypeViewModel LoadType(string name)
        => LoadTypeAsync(name).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(GetTypeAsync)} method. This current method is slated for removal in upcoming releases.")]
    EditTypeViewModel GetType(string name)
        => GetTypeAsync(name).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(AddTypeAsync)} method. This current method is slated for removal in upcoming releases.")]
    ContentTypeDefinition AddType(string name, string displayName)
        => AddTypeAsync(name, displayName).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(RemoveTypeAsync)} method. This current method is slated for removal in upcoming releases.")]
    void RemoveType(string name, bool deleteContent)
        => RemoveTypeAsync(name, deleteContent).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(AddPartToTypeAsync)} method. This current method is slated for removal in upcoming releases.")]
    void AddPartToType(string partName, string typeName)
        => AddPartToTypeAsync(partName, typeName).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(AddReusablePartToTypeAsync)} method. This current method is slated for removal in upcoming releases.")]
    void AddReusablePartToType(string name, string displayName, string description, string partName, string typeName)
        => AddReusablePartToTypeAsync(name, displayName, description, partName, typeName).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(RemovePartFromTypeAsync)} method. This current method is slated for removal in upcoming releases.")]
    void RemovePartFromType(string partName, string typeName)
        => RemovePartFromTypeAsync(partName, typeName);

    [Obsolete($"Instead, utilize the {nameof(GenerateContentTypeNameFromDisplayNameAsync)} method. This current method is slated for removal in upcoming releases.")]
    string GenerateContentTypeNameFromDisplayName(string displayName)
        => GenerateContentTypeNameFromDisplayNameAsync(displayName).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(GenerateFieldNameFromDisplayNameAsync)} method. This current method is slated for removal in upcoming releases.")]
    string GenerateFieldNameFromDisplayName(string partName, string displayName)
        => GenerateFieldNameFromDisplayNameAsync(partName, displayName).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(LoadPartsAsync)} method. This current method is slated for removal in upcoming releases.")]
    IEnumerable<EditPartViewModel> LoadParts(bool metadataPartsOnly)
        => LoadPartsAsync(metadataPartsOnly).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(GetPartsAsync)} method. This current method is slated for removal in upcoming releases.")]
    IEnumerable<EditPartViewModel> GetParts(bool metadataPartsOnly)
        => GetPartsAsync(metadataPartsOnly).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(LoadPartAsync)} method. This current method is slated for removal in upcoming releases.")]
    EditPartViewModel LoadPart(string name)
        => LoadPartAsync(name).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(GetPartAsync)} method. This current method is slated for removal in upcoming releases.")]
    EditPartViewModel GetPart(string name)
        => GetPartAsync(name).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(AddPartAsync)} method. This current method is slated for removal in upcoming releases.")]
    EditPartViewModel AddPart(CreatePartViewModel partViewModel)
        => AddPartAsync(partViewModel).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(RemovePartAsync)} method. This current method is slated for removal in upcoming releases.")]
    void RemovePart(string name)
        => RemovePartAsync(name).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(GetFieldsAsync)} method. This current method is slated for removal in upcoming releases.")]
    IEnumerable<Type> GetFields()
        => GetFieldsAsync().GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(AddFieldToPartAsync)} method. This current method is slated for removal in upcoming releases.")]
    void AddFieldToPart(string fieldName, string fieldTypeName, string partName)
        => AddFieldToPartAsync(fieldName, fieldTypeName, partName).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(AddFieldToPartAsync)} method. This current method is slated for removal in upcoming releases.")]
    void AddFieldToPart(string fieldName, string displayName, string fieldTypeName, string partName)
        => AddFieldToPartAsync(fieldName, displayName, fieldTypeName, partName).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(RemoveFieldFromPartAsync)} method. This current method is slated for removal in upcoming releases.")]
    void RemoveFieldFromPart(string fieldName, string partName)
        => RemoveFieldFromPartAsync(fieldName, partName).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(AlterFieldAsync)} method. This current method is slated for removal in upcoming releases.")]
    void AlterField(EditPartViewModel partViewModel, EditFieldViewModel fieldViewModel)
        => AlterFieldAsync(partViewModel, fieldViewModel).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(AlterTypePartAsync)} method. This current method is slated for removal in upcoming releases.")]
    void AlterTypePart(EditTypePartViewModel partViewModel)
        => AlterTypePartAsync(partViewModel).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(AlterTypePartsOrderAsync)} method. This current method is slated for removal in upcoming releases.")]
    void AlterTypePartsOrder(ContentTypeDefinition typeDefinition, string[] partNames)
        => AlterTypePartsOrderAsync(typeDefinition, partNames).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(AlterPartFieldsOrderAsync)} method. This current method is slated for removal in upcoming releases.")]
    void AlterPartFieldsOrder(ContentPartDefinition partDefinition, string[] fieldNames)
        => AlterPartFieldsOrderAsync(partDefinition, fieldNames).GetAwaiter().GetResult();
}
