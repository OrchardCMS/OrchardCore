using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Entities;

public class CacheableEntity : ICacheableEntity
{
    private readonly Dictionary<string, object> _cache = new();

    private JObject _properties = new();

    public JObject Properties
    {
        get => _properties;
        set
        {
            _properties = value ?? new JObject();
            _cache.Clear();
        }
    }

    public void Remove(string key)
    {
        AssertNotNull(key);

        _cache.Remove(key, out _);
    }

    public object Get(string key)
        => _cache.TryGetValue(key, out var value) ? value : default;

    public void Set(string key, object value)
    {
        AssertNotNull(key);

        _cache[key] = value;
    }

    private static void AssertNotNull(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException($"{nameof(key)} cannot be null or empty.");
        }
    }
}
