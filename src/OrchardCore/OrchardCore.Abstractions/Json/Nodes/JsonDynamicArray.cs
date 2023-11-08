using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;

#nullable enable

namespace System.Text.Json.Nodes;

[DebuggerDisplay("JsonDynamicArray[{Count}]")]
public class JsonDynamicArray : DynamicObject, IEnumerable<JsonNode?>
{
    private readonly JsonArray _jsonArray;

    private readonly Dictionary<string, object?> _dictionary = new();

    public JsonDynamicArray(JsonArray jsonArray) => _jsonArray = jsonArray;

    public int Count => _jsonArray.Count;

    public object? this[int index]
    {
        get
        {
            var value = GetValue(index);
            if (value is JsonDynamicValue jsonDynamicValue)
            {
                return jsonDynamicValue.JsonValue;
            }

            return value;
        }
        set
        {
            SetValue(index, value);
        }
    }

    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result)
    {
        var value = GetValue((int)indexes[0]);
        if (value is JsonDynamicValue jsonDynamicValue)
        {
            result = jsonDynamicValue.Value;
            return true;
        }

        result = value;
        return true;
    }

    public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object? value)
    {
        SetValue((int)indexes[0], value);
        return true;
    }

    public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
    {
        result = typeof(JsonArray).InvokeMember(binder.Name, BindingFlags.InvokeMethod, null, _jsonArray, args);
        return true;
    }

    public object? GetValue(int index)
    {
        if (_dictionary.TryGetValue($"{index}", out var value))
        {
            return value;
        }

        if (index >= _jsonArray.Count)
        {
            return null;
        }

        var jsonNode = _jsonArray[index];
        if (jsonNode is null)
        {
            return null;
        }

        if (jsonNode is JsonObject jsonObject)
        {
            return _dictionary[$"{index}"] = new JsonDynamicObject(jsonObject);
        }

        if (jsonNode is JsonArray jsonArray)
        {
            return _dictionary[$"{index}"] = new JsonDynamicArray(jsonArray);
        }

        if (value is JsonValue jsonValue)
        {
            return _dictionary[$"{index}"] = new JsonDynamicValue(jsonValue);
        }

        return null;
    }

    public void SetValue(int index, object? value, object? nodeValue = null)
    {
        if (value is not JsonNode)
        {
            var jsonNode = JNode.FromObject(value);
            SetValue(index, jsonNode, value);
        }

        if (value is JsonObject jsonObject)
        {
            _jsonArray[index] = jsonObject;
            _dictionary[$"{index}"] = new JsonDynamicObject(jsonObject);
            return;
        }

        if (value is JsonArray jsonArray)
        {
            _jsonArray[index] = jsonArray;
            _dictionary[$"{index}"] = new JsonDynamicArray(jsonArray);
            return;
        }

        if (value is JsonValue jsonValue)
        {
            _jsonArray[index] = jsonValue;
            _dictionary[$"{index}"] = new JsonDynamicValue(jsonValue, nodeValue);
            return;
        }
    }

    public IEnumerator<JsonNode?> GetEnumerator() => _jsonArray.AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static implicit operator JsonArray(JsonDynamicArray value) => value._jsonArray;

    public static implicit operator JsonDynamicArray(JsonArray value) => new(value);

    #region For debugging purposes only.

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        if (!int.TryParse(binder.Name, out var index))
        {
            result = null;
            return false;
        }

        result = this[index];

        return true;
    }

    public override IEnumerable<string> GetDynamicMemberNames()
    {
        if (_jsonArray.Count == 0)
        {
            return Enumerable.Empty<string>();
        }

        var list = new List<string>();
        for (var i = 0; i < _jsonArray.Count; i++)
        {
            list.Add($"{i}");
        }

        return list;
    }

    #endregion
}
