using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DisplayManagement.Shapes;

public class ShapeMetadata
{
    private (string Type, CacheContext Context) _mainCacheContext;
    private Dictionary<string, CacheContext> _cacheContexts;
    private List<Action<ShapeDisplayContext>> _displaying;
    private List<Func<IShape, Task>> _processing;
    private List<Action<ShapeDisplayContext>> _displayed;

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

    public bool IsCached => UseMainCacheContext
        ? _mainCacheContext.Context is not null
        : _cacheContexts?.ContainsKey(Type) == true;

    public IHtmlContent ChildContent { get; set; }

    internal bool UseMainCacheContext
        => _mainCacheContext.Type is null || string.Equals(_mainCacheContext.Type, Type, StringComparison.OrdinalIgnoreCase);

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

        if (UseMainCacheContext)
        {
            if (_mainCacheContext.Context is null || _mainCacheContext.Context.CacheId != cacheId)
            {
                _mainCacheContext.Context = new CacheContext(cacheId);
                _mainCacheContext.Type = Type;
            }

            return _mainCacheContext.Context;
        }

        _cacheContexts ??= new(StringComparer.OrdinalIgnoreCase);

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
        return UseMainCacheContext
            ? _mainCacheContext.Context
            : _cacheContexts?.TryGetValue(Type, out var cacheContext) == true
                ? cacheContext
                : null;
    }
}
