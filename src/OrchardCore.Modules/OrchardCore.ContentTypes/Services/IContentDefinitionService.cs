using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentTypes.Services;

public interface IContentDefinitionService
{
    Task<ContentTypeDefinition> AddTypeAsync(string name, string displayName);

    Task RemoveTypeAsync(string name, bool deleteContent);

    Task AddPartToTypeAsync(string partName, string typeName);

    Task AddReusablePartToTypeAsync(string name, string displayName, string description, string partName, string typeName);

    Task RemovePartFromTypeAsync(string partName, string typeName);

    Task<string> GenerateContentTypeNameFromDisplayNameAsync(string displayName);

    Task<string> GenerateFieldNameFromDisplayNameAsync(string partName, string displayName);

    Task RemovePartAsync(string name);

    Task<IEnumerable<Type>> GetFieldsAsync();

    Task AddFieldToPartAsync(string fieldName, string fieldTypeName, string partName);

    Task AddFieldToPartAsync(string fieldName, string displayName, string fieldTypeName, string partName);

    Task RemoveFieldFromPartAsync(string fieldName, string partName);

    Task AlterTypePartsOrderAsync(ContentTypeDefinition typeDefinition, string[] partNames);

    Task AlterPartFieldsOrderAsync(ContentPartDefinition partDefinition, string[] fieldNames);
}
