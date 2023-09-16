using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Entities;

public class CacheableEntity : ICacheableEntity
{
    private static readonly JObject _defaultProperties = new();

    private readonly Dictionary<string, object> _cache = new();

    private JObject _properties;

    public JObject Properties
    {
        get => _properties ?? _defaultProperties;
        set
        {
            _properties = value ?? _defaultProperties;
            _cache.Clear();
        }
    }

    public void Remove(string key)
    {
        AssertNotNull(key);

        _cache.Remove(key);
    }

    public object Get(string key)
    {
        AssertNotNull(key);

        return _cache.TryGetValue(key, out var value) ? value : null;
    }

    public void Set(string key, object value)
    {
        AssertNotNull(key);

        if (value is null)
        {
            return;
        }

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
