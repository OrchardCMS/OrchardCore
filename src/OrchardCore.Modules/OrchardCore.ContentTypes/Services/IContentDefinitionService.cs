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
}
