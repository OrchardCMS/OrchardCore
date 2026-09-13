using System.Linq.Expressions;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Layers.Models;
using OrchardCore.Rules;

namespace OrchardCore.Layers.Services;

/// <summary>Reads and manages tenant layers and their associated widgets.</summary>
public interface ILayerService
{
    /// <summary>Returns a localized validation error, or <see langword="null"/> for a valid name.</summary>
    string ValidateName(string name);
    /// <summary>Gets a cached layer by case-insensitive name, or <see langword="null"/> when absent. Do not modify the result.</summary>
    Task<Layer> GetLayerAsync(string name);
    /// <summary>Creates a uniquely named layer, generating a root rule identity when needed.</summary>
    Task<LayerMutationResult> CreateAsync(string name, string description, Rule rule = null);
    /// <summary>Updates metadata and optionally replaces the rule, generating a missing root identity. A null rule preserves the existing rule and its identities, or initializes a rule for a legacy layer without one.</summary>
    Task<LayerMutationResult> UpdateAsync(string name, string description, Rule rule = null);
    /// <summary>Deletes a layer only when no latest or published widget references it.</summary>
    Task<LayerMutationResult> DeleteAsync(string name);

    /// <summary>
    /// Loads the layers document from the store for updating and that should not be cached.
    /// </summary>
    Task<LayersDocument> LoadLayersAsync();

    /// <summary>
    /// Gets the layers document from the cache for sharing and that should not be updated.
    /// </summary>
    Task<LayersDocument> GetLayersAsync();

    Task<IEnumerable<ContentItem>> GetLayerWidgetsAsync(Expression<Func<ContentItemIndex, bool>> predicate);
    Task<IEnumerable<LayerMetadata>> GetLayerWidgetsMetadataAsync(Expression<Func<ContentItemIndex, bool>> predicate);

    /// <summary>
    /// Updates the store with the provided layers document and then updates the cache.
    /// </summary>
    Task UpdateAsync(LayersDocument layers);
}
