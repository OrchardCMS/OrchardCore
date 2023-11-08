using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;

#nullable enable

namespace System.Text.Json.Nodes;

[DebuggerDisplay("JsonDynamicObject[{Count}]")]
public class JsonDynamicObject : DynamicObject
{
    private readonly JsonObject _jsonObject;

    private readonly Dictionary<string, object?> _dictionary = new();

    public JsonDynamicObject(JsonObject jsonObject) => _jsonObject = jsonObject;

    public int Count => _jsonObject.Count;

    public JsonDynamicObject Merge(JsonNode? content, JsonMergeSettings? settings = null) =>
        new(_jsonObject.Merge(content, settings)?.Clone() ?? new JsonObject());

    public object? this[string key]
    {
        get
        {
            var value = GetValue(key);
            if (value is JsonDynamicValue jsonDynamicValue)
            {
                return jsonDynamicValue.JsonValue;
            }

            return value;
        }
        set
        {
            SetValue(key, value);
        }
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        var value = GetValue(binder.Name);
        if (value is JsonDynamicValue jsonDynamicValue)
        {
            result = jsonDynamicValue.Value;
            return true;
        }

        result = value;
        return true;
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        SetValue(binder.Name, value);
        return true;
    }

    public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
    {
        result = typeof(JsonObject).InvokeMember(binder.Name, BindingFlags.InvokeMethod, null, _jsonObject, args);
        return true;
    }

    public object? GetValue(string key)
    {
        if (_dictionary.TryGetValue(key, out var value))
        {
            return value;
        }

        if (!_jsonObject.TryGetPropertyValue(key, out var jsonNode))
        {
            return null;
        }

        if (jsonNode is null)
        {
            return null;
        }

        if (jsonNode is JsonObject jsonObject)
        {
            return _dictionary[key] = new JsonDynamicObject(jsonObject);
        }

        if (jsonNode is JsonArray jsonArray)
        {
            return _dictionary[key] = new JsonDynamicArray(jsonArray);
        }

        if (jsonNode is JsonValue jsonValue)
        {
            return _dictionary[key] = new JsonDynamicValue(jsonValue);
        }

        return null;
    }

    public void SetValue(string key, object? value, object? nodeValue = null)
    {
        if (value is not JsonNode)
        {
            var jsonNode = JNode.FromObject(value);
            SetValue(key, jsonNode, value);
        }

        if (value is JsonObject jsonObject)
        {
            _jsonObject[key] = jsonObject;
            _dictionary[key] = new JsonDynamicObject(jsonObject);
            return;
        }

        if (value is JsonArray jsonArray)
        {
            _jsonObject[key] = jsonArray;
            _dictionary[key] = new JsonDynamicArray(jsonArray);
            return;
        }

        if (value is JsonValue jsonValue)
        {
            _jsonObject[key] = jsonValue;
            _dictionary[key] = new JsonDynamicValue(jsonValue, nodeValue);
            return;
        }
    }

    public static implicit operator JsonObject(JsonDynamicObject value) => value._jsonObject;

    public static implicit operator JsonDynamicObject(JsonObject value) => new(value);

    public override IEnumerable<string> GetDynamicMemberNames() => _jsonObject.AsEnumerable().Select(node => node.Key);
}
