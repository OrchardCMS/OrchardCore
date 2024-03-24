using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;

#nullable enable

namespace System.Text.Json.Dynamic;

[DebuggerDisplay("JsonDynamicArray[{Count}]")]
public class JsonDynamicArray : DynamicObject, IEnumerable<JsonNode?>
{
    private readonly JsonArray _jsonArray;

    public readonly Dictionary<int, object?> _dictionary = [];

    public JsonDynamicArray() => _jsonArray = [];

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

    public bool Remove(JsonNode? item)
    {
        var index = _jsonArray.IndexOf(item);
        _dictionary.Remove(index);

        return _jsonArray.Remove(item);
    }

    public void RemoveAt(int index)
    {
        _dictionary.Remove(index);
        _jsonArray.RemoveAt(index);
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
        if (_dictionary.TryGetValue(index, out var value))
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
            return _dictionary[index] = new JsonDynamicObject(jsonObject);
        }

        if (jsonNode is JsonArray jsonArray)
        {
            return _dictionary[index] = new JsonDynamicArray(jsonArray);
        }

        if (jsonNode is JsonValue jsonValue)
        {
            return _dictionary[index] = new JsonDynamicValue(jsonValue);
        }

        return null;
    }

    public void SetValue(int index, object? value, object? nodeValue = null)
    {
        if (value is null)
        {
            _jsonArray[index] = null;
            _dictionary[index] = null;
            return;
        }

        if (value is not JsonNode)
        {
            var jsonNode = JNode.FromObject(value);
            SetValue(index, jsonNode, value);
        }

        if (value is JsonObject jsonObject)
        {
            _jsonArray[index] = jsonObject;
            _dictionary[index] = new JsonDynamicObject(jsonObject);
            return;
        }

        if (value is JsonArray jsonArray)
        {
            _jsonArray[index] = jsonArray;
            _dictionary[index] = new JsonDynamicArray(jsonArray);
            return;
        }

        if (value is JsonValue jsonValue)
        {
            _jsonArray[index] = jsonValue;
            _dictionary[index] = new JsonDynamicValue(jsonValue, nodeValue);
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

        if (!int.TryParse(binder.Name[1..^1], out var index))
        {
            result = null;
            return false;
        }

        var value = GetValue(index);
        if (value is JsonDynamicValue jsonDynamicValue)
        {
            result = jsonDynamicValue.Value;
            return true;
        }

        result = value;
        return true;
    }

    public override IEnumerable<string> GetDynamicMemberNames()
    {
        var names = new List<string>();
        for (var i = 0; i < _jsonArray.Count; i++)
        {
            if (_jsonArray[i] is null)
            {
                names.Add($"[{i}] {{null}}");
            }

            names.Add($"[{i}]");
        }

        if (names.Count == 0)
        {
            names.Add("{No Member}");
        }

        return names;
    }

    #endregion
}
