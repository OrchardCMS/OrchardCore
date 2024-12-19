using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Settings;

#nullable enable

namespace System.Text.Json.Dynamic;

[DebuggerDisplay("JsonDynamicObject[{Count}]")]
[JsonConverter(typeof(JsonDynamicJsonConverter<JsonDynamicObject>))]
public sealed class JsonDynamicObject : JsonDynamicBase
{
    private readonly JsonObject _jsonObject;
    private readonly Dictionary<string, object?> _dictionary = [];

    public JsonDynamicObject()
    {
        _jsonObject = [];
    }

    public JsonDynamicObject(JsonObject jsonObject)
    {
        _jsonObject = jsonObject;
    }

    public int Count => _jsonObject.Count;

    public override JsonNode Node => _jsonObject;

    public object? this[string key]
    {
        get => GetValue(key);
        set => SetValue(key, value);
    }

    public void Merge(JsonNode? content, JsonMergeSettings? settings = null)
    {
        _jsonObject.Merge(content, settings);
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        if (binder.Name == "{No Member}")
        {
            result = 0;
            return true;
        }

        if (binder.Name.EndsWith("{null}"))
        {
            result = "{null}";
            return true;
        }

        result = GetValue(binder.Name);
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

    public bool Remove(string key)
    {
        _dictionary.Remove(key);

        return _jsonObject.Remove(key);
    }

    public JsonNode? SelectNode(string path) => _jsonObject.SelectNode(path);

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

    public void SetValue(string key, object? value)
    {
        if (value is null)
        {
            _jsonObject[key] = null;
            _dictionary[key] = null;
            return;
        }

        if (value is not JsonNode)
        {
            value = JNode.FromObject(value);
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
            _dictionary[key] = new JsonDynamicValue(jsonValue);
            return;
        }
    }

    public static implicit operator JsonObject(JsonDynamicObject value) => value._jsonObject;

    public static implicit operator JsonDynamicObject(JsonObject value) => new(value);

    #region For debugging purposes only.

    public override IEnumerable<string> GetDynamicMemberNames()
    {
        var names = new List<string>();
        foreach (var node in _jsonObject)
        {
            if (node.Value is null)
            {
                names.Add($"{node.Key} {{null}}");
            }

            names.Add(node.Key);
        }

        if (names.Count == 0)
        {
            names.Add("{No Member}");
        }

        return names;
    }

    #endregion
}
