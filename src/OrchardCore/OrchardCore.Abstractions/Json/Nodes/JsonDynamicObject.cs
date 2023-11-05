using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

#nullable enable

namespace System.Text.Json.Nodes;

public class JsonDynamicObject : DynamicObject
{
    private readonly JsonObject _jsonObject;

    private readonly Dictionary<string, object?> _dictionary = new();

    public JsonDynamicObject(JsonObject jsonObject) => _jsonObject = jsonObject;

    public JsonDynamicObject Merge(JsonNode? content, JsonMergeSettings? settings = null) =>
        new(_jsonObject.Merge(content, settings)?.Clone() ?? new JsonObject());

    public object? this[string key]
    {
        get
        {
            if (!_jsonObject.TryGetPropertyValue(key, out var value))
            {
                return null;
            }

            if (value is JsonObject jsonObject)
            {
                return new JsonDynamicObject(jsonObject);
            }

            return value;
        }
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        TryGetValue(binder.Name, out result);
        return true;
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        TrySetValue(binder.Name, value);
        return true;
    }

    public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
    {
        result = typeof(JsonObject).InvokeMember(binder.Name, BindingFlags.InvokeMethod, null, _jsonObject, args);
        return true;
    }

    public bool TryGetValue(string key, out object? value)
    {
        if (_dictionary.TryGetValue(key, out var property))
        {
            value = property;
            return true;
        }

        if (!_jsonObject.TryGetPropertyValue(key, out var jsonNode))
        {
            value = null;
            return false;
        }

        if (jsonNode is JsonObject jsonObject)
        {
            value = _dictionary[key] = new JsonDynamicObject(jsonObject);
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

            value = _dictionary[key] = list;
        }
        else
        {
            value = _dictionary[key] = jsonNode.ToObject<object>();
        }

        return true;
    }

    public bool TrySetValue(string key, object? value)
    {
        if (value is JsonObject jsonObject)
        {
            _jsonObject[key] = jsonObject;
            _dictionary[key] = new JsonDynamicObject(jsonObject);
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

            _jsonObject[key] = jsonArray;
            _dictionary[key] = list;
        }
        else
        {
            _jsonObject[key] = JNode.FromObject(value);
            _dictionary[key] = value;
        }

        return true;
    }

    public static implicit operator JsonObject(JsonDynamicObject value) => value._jsonObject;

    public static implicit operator JsonDynamicObject(JsonObject value) => new(value);
}
