using System.Linq.Expressions;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Documents;
using OrchardCore.Layers.Indexes;
using OrchardCore.Layers.Models;
using OrchardCore.Rules;
using OrchardCore.Rules.Services;
using YesSql;

namespace OrchardCore.Layers.Services;

public class LayerService : ILayerService
{
    private readonly ISession _session;
    private readonly IDocumentManager<LayersDocument> _documentManager;
    private readonly IConditionIdGenerator _conditionIds;

    internal readonly IStringLocalizer S;

    public LayerService(ISession session, IDocumentManager<LayersDocument> documentManager, IConditionIdGenerator conditionIds, IStringLocalizer<LayerService> localizer)
    {
        _session = session;
        _documentManager = documentManager;
        _conditionIds = conditionIds;
        S = localizer;
    }

    public string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return S["The layer name is required."];
        }
        if (name.Length > 256 || name != name.Trim() || name.Any(char.IsControl))
        {
            return S["The layer name must have at most 256 characters, without surrounding whitespace or control characters."];
        }
        return null;
    }

    public async Task<Layer> GetLayerAsync(string name) => Find(await GetLayersAsync(), name);

    public async Task<LayerMutationResult> CreateAsync(string name, string description, Rule rule = null)
    {
        if (ValidateName(name) is { } error)
        {
            return new LayerMutationResult { Status = LayerMutationStatus.InvalidName, Error = error };
        }
        var document = await LoadLayersAsync();
        if (Find(document, name) is { } existing)
        {
            return new LayerMutationResult { Status = LayerMutationStatus.Conflict, Layer = existing, Error = S["The layer name already exists."] };
        }
        rule = EnsureRuleIdentity(rule);
        var layer = new Layer { Name = name, Description = description, LayerRule = rule };
        document.Layers.Add(layer);
        await UpdateAsync(document);
        return new LayerMutationResult { Status = LayerMutationStatus.Success, Layer = layer };
    }

    public async Task<LayerMutationResult> UpdateAsync(string name, string description, Rule rule = null)
    {
        if (ValidateName(name) is { } error)
        {
            return new LayerMutationResult { Status = LayerMutationStatus.InvalidName, Error = error };
        }
        var document = await LoadLayersAsync();
        var layer = Find(document, name);
        if (layer is null)
        {
            return new LayerMutationResult { Status = LayerMutationStatus.NotFound };
        }
        layer.Description = description;
        if (rule is not null || layer.LayerRule is null)
        {
            layer.LayerRule = EnsureRuleIdentity(rule);
        }
        await UpdateAsync(document);
        return new LayerMutationResult { Status = LayerMutationStatus.Success, Layer = layer };
    }

    public async Task<LayerMutationResult> DeleteAsync(string name)
    {
        var document = await LoadLayersAsync();
        var layer = Find(document, name);
        if (layer is null)
        {
            return new LayerMutationResult { Status = LayerMutationStatus.NotFound };
        }
        var widgets = await GetLayerWidgetsMetadataAsync(item => item.Latest || item.Published);
        if (widgets.Any(widget => string.Equals(widget.Layer, layer.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return new LayerMutationResult { Status = LayerMutationStatus.Referenced, Layer = layer };
        }
        document.Layers.Remove(layer);
        await UpdateAsync(document);
        return new LayerMutationResult { Status = LayerMutationStatus.Success, Layer = layer };
    }

    private Rule EnsureRuleIdentity(Rule rule)
    {
        rule ??= new Rule();
        if (string.IsNullOrEmpty(rule.ConditionId))
        {
            _conditionIds.GenerateUniqueId(rule);
        }
        return rule;
    }

    private static Layer Find(LayersDocument document, string name) => document.Layers
        .FirstOrDefault(layer => string.Equals(layer.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Loads the layers document from the store for updating and that should not be cached.
    /// </summary>
    public Task<LayersDocument> LoadLayersAsync() => _documentManager.GetOrCreateMutableAsync();

    /// <summary>
    /// Gets the layers document from the cache for sharing and that should not be updated.
    /// </summary>
    public Task<LayersDocument> GetLayersAsync() => _documentManager.GetOrCreateImmutableAsync();

    public async Task<IEnumerable<ContentItem>> GetLayerWidgetsAsync(
        Expression<Func<ContentItemIndex, bool>> predicate)
    {
        return await _session
            .Query<ContentItem, LayerMetadataIndex>()
            .With(predicate)
            .ListAsync();
    }

    public async Task<IEnumerable<LayerMetadata>> GetLayerWidgetsMetadataAsync(
        Expression<Func<ContentItemIndex, bool>> predicate)
    {
        var allWidgets = await GetLayerWidgetsAsync(predicate);

        return allWidgets
            .Select(x => x.TryGet<LayerMetadata>(out var layerMetadata) ? layerMetadata : null)
            .Where(x => x != null)
            .OrderBy(x => x.Position)
            .ToList();
    }

    /// <summary>
    /// Updates the store with the provided layers document and then updates the cache.
    /// </summary>
    public async Task UpdateAsync(LayersDocument layers)
    {
        var existing = await LoadLayersAsync();
        existing.Layers = layers.Layers;
        await _documentManager.UpdateAsync(layers);
    }
}
