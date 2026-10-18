using System.Text.Json;
using System.Text.Json.Nodes;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The values a pipeline keeps from one run to the next, such as the last value an incremental read reached, or the
/// identifier a destination gave to what it created. A run reads the values the last successful run saved; what it
/// changes is saved only when it succeeds, so a failed run starts again from the same values. Previews read the values
/// and never save them.
/// </summary>
public sealed class DataPipelineState
{
    // Steps run at the same time, so every access is locked.
    private readonly Lock _lock = new();
    private readonly Dictionary<string, JsonNode> _values;

    /// <summary>
    /// Initializes a new, empty instance of the <see cref="DataPipelineState"/> class.
    /// </summary>
    public DataPipelineState()
        : this(null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataPipelineState"/> class with saved values.
    /// </summary>
    /// <param name="values">The saved values, by key.</param>
    public DataPipelineState(IDictionary<string, JsonNode> values)
    {
        _values = new Dictionary<string, JsonNode>(StringComparer.Ordinal);

        foreach (var (key, value) in values ?? new Dictionary<string, JsonNode>())
        {
            _values[key] = value?.DeepClone();
        }
    }

    /// <summary>
    /// Gets a value indicating whether a value was set or removed since the state was read.
    /// </summary>
    public bool IsChanged { get; private set; }

    /// <summary>
    /// Gets the keys of the values.
    /// </summary>
    public IReadOnlyList<string> Keys
    {
        get
        {
            lock (_lock)
            {
                return [.. _values.Keys];
            }
        }
    }

    /// <summary>
    /// Gets a value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the value.</param>
    /// <returns>The value, or the default value of <typeparamref name="T"/> when there is none.</returns>
    public T Get<T>(string key)
    {
        return TryGet<T>(key, out var value) ? value : default;
    }

    /// <summary>
    /// Gets a value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the value.</param>
    /// <param name="value">The value, when there is one.</param>
    /// <returns><see langword="true"/> when there is a value.</returns>
    public bool TryGet<T>(string key, out T value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        JsonNode node;

        lock (_lock)
        {
            node = _values.TryGetValue(key, out var saved) ? saved?.DeepClone() : null;
        }

        if (node is not null)
        {
            value = node.Deserialize<T>(JOptions.Default);

            return true;
        }

        value = default;

        return false;
    }

    /// <summary>
    /// Sets a value, saved when the run succeeds.
    /// </summary>
    /// <typeparam name="T">The type of the value, serialized as JSON.</typeparam>
    /// <param name="key">The key of the value.</param>
    /// <param name="value">The value; <see langword="null"/> removes it.</param>
    public void Set<T>(string key, T value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (value is null)
        {
            Remove(key);

            return;
        }

        var node = JsonSerializer.SerializeToNode(value, JOptions.Default);

        lock (_lock)
        {
            _values[key] = node;
            IsChanged = true;
        }
    }

    /// <summary>
    /// Removes a value when the run succeeds.
    /// </summary>
    /// <param name="key">The key of the value.</param>
    public void Remove(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        lock (_lock)
        {
            IsChanged |= _values.Remove(key);
        }
    }

    /// <summary>
    /// Copies the values, to save them.
    /// </summary>
    /// <returns>The values, by key.</returns>
    public Dictionary<string, JsonNode> ToDictionary()
    {
        lock (_lock)
        {
            return _values.ToDictionary(pair => pair.Key, pair => pair.Value?.DeepClone(), StringComparer.Ordinal);
        }
    }
}
