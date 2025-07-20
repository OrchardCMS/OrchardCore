using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.Metadata;

/// <summary>
/// This interface provides each client with
/// a different copy of <see cref="ContentTypeDefinition"/> to work with in case
/// multiple clients do modifications.
/// </summary>
public interface IContentDefinitionManager
{
    Task<IEnumerable<ContentTypeDefinition>> LoadTypeDefinitionsAsync();

    Task<IEnumerable<ContentTypeDefinition>> ListTypeDefinitionsAsync();

    Task<IEnumerable<ContentPartDefinition>> LoadPartDefinitionsAsync();

    Task<IEnumerable<ContentPartDefinition>> ListPartDefinitionsAsync();

    Task<ContentTypeDefinition> LoadTypeDefinitionAsync(string name);

    Task<ContentTypeDefinition> GetTypeDefinitionAsync(string name);

    Task<ContentPartDefinition> LoadPartDefinitionAsync(string name);

    Task<ContentPartDefinition> GetPartDefinitionAsync(string name);

    Task DeleteTypeDefinitionAsync(string name);

    Task DeletePartDefinitionAsync(string name);

    Task StoreTypeDefinitionAsync(ContentTypeDefinition contentTypeDefinition);

    Task StorePartDefinitionAsync(ContentPartDefinition contentPartDefinition);

    /// <summary>
    /// Returns an unique identifier that is updated when content definitions have changed.
    /// </summary>
    Task<string> GetIdentifierAsync();
}
