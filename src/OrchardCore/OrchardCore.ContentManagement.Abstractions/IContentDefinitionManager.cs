using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    [Obsolete($"Instead, utilize the {nameof(LoadTypeDefinitionsAsync)} method. This current method is slated for removal in upcoming releases.")]
    IEnumerable<ContentTypeDefinition> LoadTypeDefinitions()
        => LoadTypeDefinitionsAsync().GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(ListTypeDefinitionsAsync)} method. This current method is slated for removal in upcoming releases.")]
    IEnumerable<ContentTypeDefinition> ListTypeDefinitions()
        => ListTypeDefinitionsAsync().GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(LoadPartDefinitionsAsync)} method. This current method is slated for removal in upcoming releases.")]
    IEnumerable<ContentPartDefinition> LoadPartDefinitions()
        => LoadPartDefinitionsAsync().GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(ListPartDefinitionsAsync)} method. This current method is slated for removal in upcoming releases.")]
    IEnumerable<ContentPartDefinition> ListPartDefinitions()
        => ListPartDefinitionsAsync().GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(LoadTypeDefinitionAsync)} method. This current method is slated for removal in upcoming releases.")]
    ContentTypeDefinition LoadTypeDefinition(string name)
        => LoadTypeDefinitionAsync(name).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(GetTypeDefinitionAsync)} method. This current method is slated for removal in upcoming releases.")]
    ContentTypeDefinition GetTypeDefinition(string name)
        => GetTypeDefinitionAsync(name).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(LoadPartDefinitionAsync)} method. This current method is slated for removal in upcoming releases.")]
    ContentPartDefinition LoadPartDefinition(string name)
        => LoadPartDefinitionAsync(name).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(GetPartDefinitionAsync)} method. This current method is slated for removal in upcoming releases.")]
    ContentPartDefinition GetPartDefinition(string name)
        => GetPartDefinitionAsync(name).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(DeleteTypeDefinitionAsync)} method. This current method is slated for removal in upcoming releases.")]
    void DeleteTypeDefinition(string name)
        => DeleteTypeDefinitionAsync(name).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(DeletePartDefinitionAsync)} method. This current method is slated for removal in upcoming releases.")]
    void DeletePartDefinition(string name)
        => DeletePartDefinitionAsync(name).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(StoreTypeDefinitionAsync)} method. This current method is slated for removal in upcoming releases.")]
    void StoreTypeDefinition(ContentTypeDefinition contentTypeDefinition)
        => StoreTypeDefinitionAsync(contentTypeDefinition).GetAwaiter().GetResult();

    [Obsolete($"Instead, utilize the {nameof(StorePartDefinitionAsync)} method. This current method is slated for removal in upcoming releases.")]
    void StorePartDefinition(ContentPartDefinition contentPartDefinition)
        => StorePartDefinitionAsync(contentPartDefinition).GetAwaiter().GetResult();
}
