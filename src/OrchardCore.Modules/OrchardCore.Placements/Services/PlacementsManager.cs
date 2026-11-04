using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;

namespace OrchardCore.Placements.Services;

/// <summary>Manages tenant-owned placement overrides through the selected database or file store.</summary>
public sealed class PlacementsManager
{
    private readonly IPlacementStore _placementStore;
    private readonly HashSet<string> _filterKeys;

    internal readonly IStringLocalizer S;

    /// <summary>Creates a manager using the active store, registered filters and localized validation messages.</summary>
    public PlacementsManager(IPlacementStore placementStore, IEnumerable<IPlacementNodeFilterProvider> filterProviders,
        IStringLocalizer<PlacementsManager> localizer)
    {
        _placementStore = placementStore;
        _filterKeys = filterProviders.Select(provider => provider.Key).ToHashSet(StringComparer.Ordinal);
        S = localizer;
    }

    /// <summary>Reads all cached placement overrides. Callers must not modify the returned definitions.</summary>
    public async Task<IReadOnlyDictionary<string, PlacementNode[]>> ListShapePlacementsAsync()
    {
        var document = await _placementStore.GetPlacementsAsync();
        return document.Placements;
    }

    /// <summary>Reads cached rules for a case-insensitive shape type, or null when no override exists.</summary>
    public async Task<PlacementNode[]> GetShapePlacementsAsync(string shapeType)
    {
        var document = await _placementStore.GetPlacementsAsync();
        return document.Placements.TryGetValue(shapeType, out var nodes) ? nodes : null;
    }

    /// <summary>Upserts placement rules for recipe/programmatic callers, preserving their import semantics.</summary>
    public async Task UpdateShapePlacementsAsync(string shapeType, IEnumerable<PlacementNode> placementNodes)
    {
        var document = await _placementStore.LoadPlacementsAsync();
        document.Placements[shapeType] = placementNodes.ToArray();
        await _placementStore.SavePlacementsAsync(document);
    }

    /// <summary>Removes stored rules if present, invalidating only the selected tenant store.</summary>
    public async Task RemoveShapePlacementsAsync(string shapeType)
    {
        var document = await _placementStore.LoadPlacementsAsync();
        if (document.Placements.Remove(shapeType))
        {
            await _placementStore.SavePlacementsAsync(document);
        }
    }

    internal IReadOnlyList<string> GetFilterKeys() => _filterKeys.Order(StringComparer.Ordinal).ToArray();

    internal Dictionary<string, string[]> Validate(string shapeType, PlacementNode[] nodes)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(shapeType))
        {
            errors["shapeType"] = [S["The Shape type can't be empty."]];
        }
        else if (shapeType.Length > 256 || shapeType != shapeType.Trim() || shapeType.Any(char.IsControl))
        {
            errors["shapeType"] = [S["The shape type must have at most 256 characters without surrounding whitespace or control characters."]];
        }
        if (nodes is null)
        {
            errors["nodes"] = [S["Provide a placement rules array."]];
            return errors;
        }
        for (var index = 0; index < nodes.Length; index++)
        {
            var node = nodes[index];
            var path = $"nodes[{index}]";
            if (node is null || IsEmpty(node))
            {
                errors[path] = [S["A valid placement must contain place, shape, wrappers or alternates."]];
                continue;
            }
            if (node.Alternates?.Any(string.IsNullOrWhiteSpace) == true || node.Wrappers?.Any(string.IsNullOrWhiteSpace) == true)
            {
                errors[path] = [S["Alternate and wrapper names cannot be empty."]];
            }
            if (node.Filters is null)
            {
                errors[path] = [S["Placement filters cannot be null."]];
                continue;
            }
            foreach (var filter in node.Filters)
            {
                if (!_filterKeys.Contains(filter.Key))
                {
                    errors[path + "." + filter.Key] = [S["The placement filter '{0}' is not registered by an enabled feature.", filter.Key]];
                }
                else if (filter.Key is "path" or "contentType" or "contentPart")
                {
                    var value = JNode.FromObject(filter.Value);
                    if (!IsString(value) && (value is not JsonArray values || values.Any(item => !IsString(item))))
                    {
                        errors[path + "." + filter.Key] = [S["This placement filter requires a string or an array of strings."]];
                    }
                }
            }
        }
        return errors;
    }

    internal async Task<PlacementMutationResult> SaveAsync(string shapeType, PlacementNode[] nodes, bool creating = false, bool requireExisting = false)
    {
        var errors = Validate(shapeType, nodes);
        if (errors.Count > 0)
        {
            return new() { Status = PlacementMutationStatus.Invalid, Errors = errors };
        }
        var document = await _placementStore.LoadPlacementsAsync();
        var entry = document.Placements.FirstOrDefault(entry => string.Equals(entry.Key, shapeType, StringComparison.OrdinalIgnoreCase));
        if (entry.Key is null && requireExisting && nodes.Length > 0)
        {
            return new() { Status = PlacementMutationStatus.NotFound };
        }
        if (entry.Key is not null && creating)
        {
            return new()
            {
                Status = Equivalent(entry.Value, nodes) ? PlacementMutationStatus.Existing : PlacementMutationStatus.Conflict,
                ShapeType = entry.Key, Nodes = entry.Value,
            };
        }
        if (nodes.Length == 0)
        {
            if (entry.Key is not null)
            {
                document.Placements.Remove(entry.Key);
                await _placementStore.SavePlacementsAsync(document);
            }
            return new() { Status = PlacementMutationStatus.Deleted, ShapeType = shapeType, Nodes = [] };
        }
        if (entry.Key is not null && Equivalent(entry.Value, nodes))
        {
            return new() { Status = PlacementMutationStatus.Existing, ShapeType = entry.Key, Nodes = entry.Value };
        }
        var name = entry.Key ?? shapeType;
        document.Placements[name] = nodes;
        await _placementStore.SavePlacementsAsync(document);
        return new() { Status = PlacementMutationStatus.Saved, ShapeType = name, Nodes = nodes };
    }

    private static bool IsEmpty(PlacementNode node) => string.IsNullOrEmpty(node.Location)
        && string.IsNullOrEmpty(node.ShapeType) && (node.Alternates is null || node.Alternates.Length == 0)
        && (node.Wrappers is null || node.Wrappers.Length == 0);

    private static bool IsString(JsonNode value) => value is JsonValue scalar && scalar.TryGetValue<string>(out _);

    private static bool Equivalent(PlacementNode[] left, PlacementNode[] right) =>
        JsonNode.DeepEquals(JNode.FromObject(left), JNode.FromObject(right));
}

internal enum PlacementMutationStatus
{
    Saved,
    Existing,
    Conflict,
    Invalid,
    NotFound,
    Deleted,
}

internal sealed class PlacementMutationResult
{
    public PlacementMutationStatus Status { get; init; }
    public string ShapeType { get; init; }
    public PlacementNode[] Nodes { get; init; }
    public Dictionary<string, string[]> Errors { get; init; } = [];
}
