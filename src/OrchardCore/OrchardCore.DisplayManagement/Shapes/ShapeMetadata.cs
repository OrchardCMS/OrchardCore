using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DisplayManagement.Shapes;

public class ShapeMetadata
{
    private Dictionary<string, CacheContext> _cacheContexts;
    private List<Action<ShapeDisplayContext>> _displaying;
    private List<Func<IShape, Task>> _processing;
    private List<Action<ShapeDisplayContext>> _displayed;

    public ShapeMetadata()
    {
    }

    public string Type { get; set; }
    public string DisplayType { get; set; }
    public string Position { get; set; }
    public string Tab { get; set; }
    public string Card { get; set; }
    public string Column { get; set; }
    public string PlacementSource { get; set; }
    public string Prefix { get; set; }
    public string Name { get; set; }
    public string Differentiator { get; set; }
    public AlternatesCollection Wrappers { get; set; } = [];
    public AlternatesCollection Alternates { get; set; } = [];
    public bool IsCached => _cacheContexts?.ContainsKey(Type) ?? false;
    public IHtmlContent ChildContent { get; set; }

    // The casts in (IReadOnlyList<T>)_displaying ?? [] are important as they convert [] to Array.Empty.
    // It would use List<T> otherwise which is not what we want here, we don't want to allocate.

    /// <summary>
    /// Event use for a specific shape instance.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<Action<ShapeDisplayContext>> Displaying => (IReadOnlyList<Action<ShapeDisplayContext>>)_displaying ?? [];

    /// <summary>
    /// Event use for a specific shape instance.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<Func<IShape, Task>> ProcessingAsync => (IReadOnlyList<Func<IShape, Task>>)_processing ?? [];

    /// <summary>
    /// Event use for a specific shape instance.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<Action<ShapeDisplayContext>> Displayed => (IReadOnlyList<Action<ShapeDisplayContext>>)_displayed ?? [];

    [JsonIgnore]
    public IReadOnlyList<string> BindingSources { get; set; } = [];

    public void OnDisplaying(Action<ShapeDisplayContext> context)
    {
        _displaying ??= new List<Action<ShapeDisplayContext>>();
        _displaying.Add(context);
    }

    public void OnProcessing(Func<IShape, Task> context)
    {
        _processing ??= new List<Func<IShape, Task>>();
        _processing.Add(context);
    }

    public void OnDisplayed(Action<ShapeDisplayContext> context)
    {
        _displayed ??= new List<Action<ShapeDisplayContext>>();
        _displayed.Add(context);
    }

    /// <summary>
    /// Marks this shape to be cached.
    /// </summary>
    public CacheContext Cache(string cacheId)
    {
        ArgumentException.ThrowIfNullOrEmpty(cacheId);

        cacheId = $"{Type}_{cacheId}";

        _cacheContexts ??= new Dictionary<string, CacheContext>(StringComparer.OrdinalIgnoreCase);

        if (!_cacheContexts.TryGetValue(Type, out var cacheContext) || cacheContext.CacheId != cacheId)
        {
            _cacheContexts[Type] = cacheContext = new CacheContext(cacheId);
        }

        return cacheContext;
    }

    /// <summary>
    /// Returns the <see cref="CacheContext"/> instance if the shape is cached.
    /// </summary>
    public CacheContext Cache()
    {
        return _cacheContexts?.TryGetValue(Type, out var cacheContext) == true ? cacheContext : null;
    }
}
