using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

#nullable enable

namespace System.Text.Json.Nodes;

public class JsonDynamicObject : DynamicObject, IDictionary<string, object?>
{
    private readonly IDictionary<string, object?> _obj = new Dictionary<string, object?>();

    public JsonDynamicObject() { }

    public JsonDynamicObject(JsonObject jsonObject) => JsonObject = jsonObject;

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
            if (TryGetValue(key, out var value))
            {
                return value;
            }

            return null;
        }
        set
        {
            Add(key, value);
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

    public void Add(string key, object? value)
    {
        if (value is JsonObject jsonObject)
        {
            JsonObject.Add(key, jsonObject);
            _obj[key] = new JsonDynamicObject(jsonObject);
        }
        else if (value is JsonNode jsonNode)
        {
            _obj[key] = jsonNode.Deserialize<object>();
            JsonObject.Add(key, jsonNode);
        }
        else
        {
            _obj[key] = value;
            JsonObject.Add(key, JNode.FromObject(value));
        }
    }

    public bool ContainsKey(string key) => JsonObject.ContainsKey(key);

    public ICollection<string> Keys => ToDictionary().Keys;

    public bool Remove(string key)
    {
        _obj.Remove(key);
        return JsonObject.Remove(key);
    }

    public bool TryGetValue(string key, out object? value)
    {
        if (_obj.TryGetValue(key, out var existing))
        {
            value = existing;
            return true;
        }

        if (!JsonObject.TryGetPropertyValue(key, out var jsonNode))
        {
            value = null;
            return false;
        }

        if (jsonNode is JsonObject jsonObject)
        {
            value = _obj[key] = new JsonDynamicObject(jsonObject);
        }
        else
        {
            value = _obj[key] = jsonNode.ToObject<object>();
        }

        return true;
    }

    public ICollection<object?> Values => ToDictionary().Values;

    public void Add(KeyValuePair<string, object?> item) => Add(item.Key, item.Value);

    public void Clear()
    {
        JsonObject.Clear();
        _obj.Clear();
    }

    public bool Contains(KeyValuePair<string, object?> item) => _obj.TryGetValue(item.Key, out var value) && value == item.Value;

    public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) => ToDictionary().CopyTo(array, arrayIndex);

    public int Count => JsonObject.Count;

    public bool IsReadOnly { get; set; }

    public bool Remove(KeyValuePair<string, object?> item) => Contains(item) && Remove(item.Key);

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => ToDictionary().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private IDictionary<string, object?> ToDictionary()
    {
        foreach (var node in JsonObject)
        {
            if (!_obj.ContainsKey(node.Key))
            {
                if (node.Value is JsonObject jsonObject)
                {
                    _obj[node.Key] = new JsonDynamicObject(jsonObject);
                }
                else
                {
                    _obj[node.Key] = node.Value.ToObject<object>();
                }
            }
        }

        return _obj;
    }
}
