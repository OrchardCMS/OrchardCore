using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Entities;

public class CacheableEntity : ICacheableEntity
{
    private static readonly JObject _defaultProperties = new();

    private readonly ConcurrentDictionary<string, object> _cache = new();

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
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException($"{nameof(key)} cannot be null or empty.");
        }

        _cache.Remove(key, out _);
    }

    public object Get(string key) => _cache.TryGetValue(key, out var value)
        ? value
        : default;

    public void Set(string key, object value) => _cache.TryAdd(key, value);
}
