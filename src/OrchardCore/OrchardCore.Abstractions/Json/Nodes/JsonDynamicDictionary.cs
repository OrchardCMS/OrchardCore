using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

#nullable enable

namespace System.Text.Json.Nodes;

public class JsonDynamicDictionary : DynamicObject //, IDictionary<string, object?>
{
    private readonly Dictionary<string, object?> _dictionary = new();

    //public JsonDynamicDictionary() => _dictionary = new Dictionary<string, object?>();

    //public JsonDynamicDictionary(Dictionary<string, object?> dictionary)
    //{
    //    _dictionary = dictionary;

    //    JsonObject = new(_dictionary.AsEnumerable());
    //}

    public JsonDynamicDictionary() { }

    public JsonDynamicDictionary(JsonObject jsonObject)
    {
        JsonObject = jsonObject;

        _dictionary = jsonObject.ToDictionary(node => node.Key, node => node.Value as object);
    }

    public JsonObject JsonObject { get; set; } = new JsonObject();

    public T? ToObject<T>() => JsonObject.ToObject<T>();

    public JsonDynamicObject Clone() => new(JsonObject.Clone() ?? new JsonObject());

    public JsonDynamicObject Merge(JsonNode? content, JsonMergeSettings? settings = null) =>
        new(JsonObject.Merge(content, settings)?.Clone() ?? new JsonObject());

    public JsonObject AsObject() => JsonObject;



    public object? this[string key]
    {
        get
        {
            _dictionary.TryGetValue(key, out var result);
            return Wrap(result);
        }
        set
        {
            _dictionary[key] = Unwrap(value);
        }
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        result = this[binder.Name];
        return true;
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        this[binder.Name] = value;
        return true;
    }

    public static object? Wrap(object? value)
    {
        var obj = value as JsonDynamicDictionary;
        if (obj is not null)
        {
            return new JsonDynamicDictionary(obj.JsonObject);
        }

        return value;
    }

    public static object? Unwrap(object? value)
    {
        var dictionary = value as JsonDynamicDictionary;
        if (dictionary is not null)
        {
            return dictionary._dictionary;
        }

        return value;
    }

    public void Add(string key, object? value) => Add(key, Unwrap(value));

    public bool ContainsKey(string key) => _dictionary.ContainsKey(key);

    public ICollection<string> Keys => _dictionary.Keys;

    public bool Remove(string key) => _dictionary.Remove(key);

    public bool TryGetValue(string key, out object? value) => _dictionary.TryGetValue(key, out value);

    public ICollection<object?> Values => _dictionary.Values;

    //public void Add(KeyValuePair<string, object?> item) => _dictionary.Add(item);

    public void Clear() => _dictionary.Clear();

    //public bool Contains(KeyValuePair<string, object?> item) => _dictionary.Contains(item);

    //public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) => _dictionary.CopyTo(array, arrayIndex);

    public int Count => _dictionary.Count;

    //public bool IsReadOnly => _dictionary.IsReadOnly;

    //public bool Remove(KeyValuePair<string, object?> item) => _dictionary.Remove(item);

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _dictionary.GetEnumerator();

    //IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
