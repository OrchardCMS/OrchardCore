using System.Collections.Generic;
using System.Dynamic;

#nullable enable

namespace System.Text.Json.Nodes;

public class JsonDynamicObject : DynamicObject
{
    private readonly Dictionary<string, object?> _obj = new();

    public JsonDynamicObject(JsonObject jsonObject) => JsonObject = jsonObject;

    public JsonObject JsonObject { get; set; }

    public T? ToObject<T>() => JsonObject.ToObject<T>();

    public JsonDynamicObject Clone() => new(JsonObject.Clone() ?? new JsonObject());

    public JsonDynamicObject Merge(JsonNode? content, JsonMergeSettings? settings = null) =>
        new(JsonObject.Merge(content, settings)?.Clone() ?? new JsonObject());

    public JsonObject AsObject() => JsonObject;

    public object? this[string key]
    {
        get
        {
            if (!JsonObject.TryGetPropertyValue(key, out var value))
            {
                return null;
            }

            if (value is JsonObject jsonObject)
            {
                return new JsonDynamicObject(jsonObject);
            }

            return value;
        }
        set
        {
            TrySetValue(key, value);
        }
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result) => TryGetValue(binder.Name, out result);

    public override bool TrySetMember(SetMemberBinder binder, object? value) => TrySetValue(binder.Name, value);

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
        else if (jsonNode is JsonArray jsonArray)
        {
            var list = new List<object?>();
            for (var i = 0; i < jsonArray.Count; i++)
            {
                if (jsonArray[i] is JsonObject jsonItem)
                {
                    list.Add(new JsonDynamicObject(jsonItem));
                }
                else
                {
                    list.Add(jsonArray[i].ToObject<object>());
                }
            }

            value = _obj[key] = list;
        }
        else
        {
            value = _obj[key] = jsonNode.ToObject<object>();
        }

        return true;
    }

    public bool TrySetValue(string key, object? value)
    {
        if (value is JsonObject jsonObject)
        {
            JsonObject[key] = jsonObject;
            _obj[key] = new JsonDynamicObject(jsonObject);
        }
        else if (value is JsonArray jsonArray)
        {
            var list = new List<object?>();
            for (var i = 0; i < jsonArray.Count; i++)
            {
                if (jsonArray[i] is JsonObject jsonItem)
                {
                    list.Add(new JsonDynamicObject(jsonItem));
                }
                else
                {
                    list.Add(jsonArray[i].ToObject<object>());
                }
            }

            JsonObject[key] = jsonArray;
            _obj[key] = list;
        }
        else
        {
            JsonObject[key] = JNode.FromObject(value);
            _obj[key] = value;
        }

        return true;
    }

    public bool ContainsKey(string key) => JsonObject.ContainsKey(key);

    public bool Remove(string key)
    {
        _obj.Remove(key);
        return JsonObject.Remove(key);
    }

    public void Clear()
    {
        JsonObject.Clear();
        _obj.Clear();
    }

    public int Count => JsonObject.Count;
}
