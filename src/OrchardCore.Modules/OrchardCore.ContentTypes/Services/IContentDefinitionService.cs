using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.ViewModels;

namespace OrchardCore.ContentTypes.Services
{
    public interface IContentDefinitionService
    {
        [Obsolete($"Instead, utilize the {nameof(LoadTypesAsync)} method. This current method is slated for removal in upcoming releases.")]
        IEnumerable<EditTypeViewModel> LoadTypes();

        [Obsolete($"Instead, utilize the {nameof(GetTypesAsync)} method. This current method is slated for removal in upcoming releases.")]
        IEnumerable<EditTypeViewModel> GetTypes();

        [Obsolete($"Instead, utilize the {nameof(LoadTypeAsync)} method. This current method is slated for removal in upcoming releases.")]
        EditTypeViewModel LoadType(string name);

        [Obsolete($"Instead, utilize the {nameof(GetTypeAsync)} method. This current method is slated for removal in upcoming releases.")]
        EditTypeViewModel GetType(string name);

        [Obsolete($"Instead, utilize the {nameof(AddTypeAsync)} method. This current method is slated for removal in upcoming releases.")]
        ContentTypeDefinition AddType(string name, string displayName);

        [Obsolete($"Instead, utilize the {nameof(RemoveTypeAsync)} method. This current method is slated for removal in upcoming releases.")]
        void RemoveType(string name, bool deleteContent);

        [Obsolete($"Instead, utilize the {nameof(AddPartToTypeAsync)} method. This current method is slated for removal in upcoming releases.")]
        void AddPartToType(string partName, string typeName);

        [Obsolete($"Instead, utilize the {nameof(AddReusablePartToTypeAsync)} method. This current method is slated for removal in upcoming releases.")]
        void AddReusablePartToType(string name, string displayName, string description, string partName, string typeName);

        [Obsolete($"Instead, utilize the {nameof(RemovePartFromTypeAsync)} method. This current method is slated for removal in upcoming releases.")]
        void RemovePartFromType(string partName, string typeName);

        [Obsolete($"Instead, utilize the {nameof(GenerateContentTypeNameFromDisplayNameAsync)} method. This current method is slated for removal in upcoming releases.")]
        string GenerateContentTypeNameFromDisplayName(string displayName);

        [Obsolete($"Instead, utilize the {nameof(GenerateFieldNameFromDisplayNameAsync)} method. This current method is slated for removal in upcoming releases.")]
        string GenerateFieldNameFromDisplayName(string partName, string displayName);

        [Obsolete($"Instead, utilize the {nameof(LoadPartsAsync)} method. This current method is slated for removal in upcoming releases.")]
        IEnumerable<EditPartViewModel> LoadParts(bool metadataPartsOnly);

        [Obsolete($"Instead, utilize the {nameof(GetPartsAsync)} method. This current method is slated for removal in upcoming releases.")]
        IEnumerable<EditPartViewModel> GetParts(bool metadataPartsOnly);

        [Obsolete($"Instead, utilize the {nameof(LoadPartAsync)} method. This current method is slated for removal in upcoming releases.")]
        EditPartViewModel LoadPart(string name);

        [Obsolete($"Instead, utilize the {nameof(GetPartAsync)} method. This current method is slated for removal in upcoming releases.")]
        EditPartViewModel GetPart(string name);

        [Obsolete($"Instead, utilize the {nameof(AddPartAsync)} method. This current method is slated for removal in upcoming releases.")]
        EditPartViewModel AddPart(CreatePartViewModel partViewModel);

        [Obsolete($"Instead, utilize the {nameof(RemovePartAsync)} method. This current method is slated for removal in upcoming releases.")]
        void RemovePart(string name);

        [Obsolete($"Instead, utilize the {nameof(GetFieldsAsync)} method. This current method is slated for removal in upcoming releases.")]
        IEnumerable<Type> GetFields();

        [Obsolete($"Instead, utilize the {nameof(AddFieldToPartAsync)} method. This current method is slated for removal in upcoming releases.")]
        void AddFieldToPart(string fieldName, string fieldTypeName, string partName);

        [Obsolete($"Instead, utilize the {nameof(AddFieldToPartAsync)} method. This current method is slated for removal in upcoming releases.")]
        void AddFieldToPart(string fieldName, string displayName, string fieldTypeName, string partName);

        [Obsolete($"Instead, utilize the {nameof(RemoveFieldFromPartAsync)} method. This current method is slated for removal in upcoming releases.")]
        void RemoveFieldFromPart(string fieldName, string partName);

        [Obsolete($"Instead, utilize the {nameof(AlterFieldAsync)} method. This current method is slated for removal in upcoming releases.")]
        void AlterField(EditPartViewModel partViewModel, EditFieldViewModel fieldViewModel);

        [Obsolete($"Instead, utilize the {nameof(AlterTypePartAsync)} method. This current method is slated for removal in upcoming releases.")]
        void AlterTypePart(EditTypePartViewModel partViewModel);

        [Obsolete($"Instead, utilize the {nameof(AlterTypePartsOrderAsync)} method. This current method is slated for removal in upcoming releases.")]
        void AlterTypePartsOrder(ContentTypeDefinition typeDefinition, string[] partNames);

        [Obsolete($"Instead, utilize the {nameof(AlterPartFieldsOrderAsync)} method. This current method is slated for removal in upcoming releases.")]
        void AlterPartFieldsOrder(ContentPartDefinition partDefinition, string[] fieldNames);


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
    }
}
