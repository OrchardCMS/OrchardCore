namespace OrchardCore.Navigation;

/// <summary>
/// Collects the nodes of a breadcrumb trail. Every <see cref="IBreadcrumbProvider"/> registered on the tenant is given
/// the same builder, so a module can append a node to, or remove a node from, a trail described by another module.
/// </summary>
public sealed class BreadcrumbBuilder
{
    private static readonly Dictionary<string, object> s_emptyData = [];

    private readonly List<BreadcrumbItem> _items = [];

    public BreadcrumbBuilder(string name, IReadOnlyDictionary<string, object> data = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        Name = name;
        Data = data ?? s_emptyData;
    }

    /// <summary>
    /// Gets the name of the breadcrumb being built. e.g., <c>Contents.Edit</c>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the contextual data the page provided when it rendered the breadcrumb. It typically holds the entity the
    /// page is about, e.g. the content item being edited.
    /// </summary>
    public IReadOnlyDictionary<string, object> Data { get; }

    /// <summary>
    /// Gets the nodes added so far, in the order they were added. The nodes are ordered by their position by the
    /// <see cref="IBreadcrumbManager"/>.
    /// </summary>
    public IReadOnlyList<BreadcrumbItem> Items => _items;

    /// <summary>
    /// Adds a node to the trail.
    /// </summary>
    /// <param name="text">The text to display for the node.</param>
    /// <param name="itemBuilder">An optional delegate to configure the node.</param>
    public BreadcrumbBuilder Add(string text, Action<BreadcrumbItemBuilder> itemBuilder = null)
        => Add(text, null, itemBuilder);

    /// <summary>
    /// Adds a node to the trail at the given position.
    /// </summary>
    /// <param name="text">The text to display for the node.</param>
    /// <param name="position">The relative position of the node among the other nodes of the trail.</param>
    /// <param name="itemBuilder">An optional delegate to configure the node.</param>
    public BreadcrumbBuilder Add(string text, string position, Action<BreadcrumbItemBuilder> itemBuilder = null)
    {
        var item = new BreadcrumbItem
        {
            Text = text,
            Position = position,
        };

        itemBuilder?.Invoke(new BreadcrumbItemBuilder(item));

        _items.Add(item);

        return this;
    }

    /// <summary>
    /// Adds an existing node to the trail.
    /// </summary>
    public BreadcrumbBuilder Add(BreadcrumbItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        _items.Add(item);

        return this;
    }

    /// <summary>
    /// Removes every node matching the given predicate from the trail.
    /// </summary>
    public BreadcrumbBuilder Remove(Predicate<BreadcrumbItem> match)
    {
        ArgumentNullException.ThrowIfNull(match);

        _items.RemoveAll(match);

        return this;
    }

    /// <summary>
    /// Gets the contextual value stored under the given key, when it exists and is of the expected type.
    /// </summary>
    public bool TryGetData<T>(string key, out T value)
    {
        if (Data.TryGetValue(key, out var data) && data is T typedData)
        {
            value = typedData;

            return true;
        }

        value = default;

        return false;
    }

    /// <summary>
    /// Gets the contextual value stored under the given key, or the default value of <typeparamref name="T"/> when it
    /// does not exist or is not of the expected type.
    /// </summary>
    public T GetData<T>(string key)
        => TryGetData<T>(key, out var value) ? value : default;

    /// <summary>
    /// Returns the nodes of the trail.
    /// </summary>
    public List<BreadcrumbItem> Build()
        => [.. _items];
}
