using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentTypes;

/// <summary>
/// Provides a reusable service for content definition operations.
/// This interface provides methods that trigger content definition events and can be used across modules.
/// </summary>
public interface IContentDefinitionService
{
    /// <summary>
    /// Creates a new content type with the specified name and display name.
    /// </summary>
    /// <param name="name">The technical name of the content type. If null or whitespace, it will be generated from the display name.</param>
    /// <param name="displayName">The display name of the content type.</param>
    /// <returns>The created content type definition.</returns>
    Task<ContentTypeDefinition> AddTypeAsync(string name, string displayName);

    /// <summary>
    /// Removes a content type.
    /// </summary>
    /// <param name="name">The technical name of the content type to remove.</param>
    /// <param name="deleteContent">Whether to delete the associated content items.</param>
    Task RemoveTypeAsync(string name, bool deleteContent);

    /// <summary>
    /// Adds a part to a content type.
    /// </summary>
    /// <param name="partName">The name of the part to add.</param>
    /// <param name="typeName">The name of the content type to add the part to.</param>
    Task AddPartToTypeAsync(string partName, string typeName);

    /// <summary>
    /// Adds a reusable part to a content type with a custom name.
    /// </summary>
    /// <param name="name">The custom name for the part instance.</param>
    /// <param name="displayName">The display name for the part instance.</param>
    /// <param name="description">The description for the part instance.</param>
    /// <param name="partName">The name of the part definition to add.</param>
    /// <param name="typeName">The name of the content type to add the part to.</param>
    Task AddReusablePartToTypeAsync(string name, string displayName, string description, string partName, string typeName);

    /// <summary>
    /// Removes a part from a content type.
    /// </summary>
    /// <param name="partName">The name of the part to remove.</param>
    /// <param name="typeName">The name of the content type to remove the part from.</param>
    Task RemovePartFromTypeAsync(string partName, string typeName);

    /// <summary>
    /// Creates a new content part definition.
    /// </summary>
    /// <param name="name">The technical name of the part to create.</param>
    /// <returns>The created content part definition.</returns>
    Task<ContentPartDefinition> AddPartAsync(string name);

    /// <summary>
    /// Removes a content part definition.
    /// </summary>
    /// <param name="name">The technical name of the part to remove.</param>
    Task RemovePartAsync(string name);

    /// <summary>
    /// Adds a field to a content part.
    /// </summary>
    /// <param name="fieldName">The technical name of the field.</param>
    /// <param name="fieldTypeName">The type name of the field.</param>
    /// <param name="partName">The name of the part to add the field to.</param>
    Task AddFieldToPartAsync(string fieldName, string fieldTypeName, string partName);

    /// <summary>
    /// Adds a field to a content part with a display name.
    /// </summary>
    /// <param name="fieldName">The technical name of the field.</param>
    /// <param name="displayName">The display name of the field.</param>
    /// <param name="fieldTypeName">The type name of the field.</param>
    /// <param name="partName">The name of the part to add the field to.</param>
    Task AddFieldToPartAsync(string fieldName, string displayName, string fieldTypeName, string partName);

    /// <summary>
    /// Removes a field from a content part.
    /// </summary>
    /// <param name="fieldName">The name of the field to remove.</param>
    /// <param name="partName">The name of the part to remove the field from.</param>
    Task RemoveFieldFromPartAsync(string fieldName, string partName);

    /// <summary>
    /// Alters a field's settings in a content part.
    /// </summary>
    /// <param name="context">The context containing field alteration details.</param>
    Task AlterFieldAsync(AlterFieldContext context);

    /// <summary>
    /// Alters a type part's settings in a content type.
    /// </summary>
    /// <param name="context">The context containing type part alteration details.</param>
    Task AlterTypePartAsync(AlterTypePartContext context);

    /// <summary>
    /// Alters the order of parts in a content type.
    /// </summary>
    /// <param name="typeDefinition">The content type definition to alter.</param>
    /// <param name="partNames">The ordered array of part names.</param>
    Task AlterTypePartsOrderAsync(ContentTypeDefinition typeDefinition, string[] partNames);

    /// <summary>
    /// Alters the order of fields in a content part.
    /// </summary>
    /// <param name="partDefinition">The content part definition to alter.</param>
    /// <param name="fieldNames">The ordered array of field names.</param>
    Task AlterPartFieldsOrderAsync(ContentPartDefinition partDefinition, string[] fieldNames);

    /// <summary>
    /// Generates a unique content type name from a display name.
    /// </summary>
    /// <param name="displayName">The display name to generate from.</param>
    /// <returns>A unique content type name.</returns>
    Task<string> GenerateContentTypeNameFromDisplayNameAsync(string displayName);

    /// <summary>
    /// Generates a unique field name from a display name within a part.
    /// </summary>
    /// <param name="partName">The name of the part containing the field.</param>
    /// <param name="displayName">The display name to generate from.</param>
    /// <returns>A unique field name.</returns>
    Task<string> GenerateFieldNameFromDisplayNameAsync(string partName, string displayName);
}
