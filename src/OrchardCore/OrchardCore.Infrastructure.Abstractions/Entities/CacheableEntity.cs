using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Entities;

public class CacheableEntity : ICacheableEntity
{
    private JObject _properties = new();

    private Dictionary<string, object> _cache { get; } = new Dictionary<string, object>();

    public JObject Properties
    {
        get
        {
            return _properties;
        }
        set
        {
            _properties = value ?? new JObject();
            _cache.Clear();
        }
    }

    public void Remove(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));

        _cache.Remove(key);
    }

    public object Get(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));

        if (_cache.TryGetValue(key, out var value))
        {
            return value;
        }

        return null;
    }

    public void Set(string key, object value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));

        if (value is null)
        {
            return;
        }

        _cache[key] = value;
    }
}
