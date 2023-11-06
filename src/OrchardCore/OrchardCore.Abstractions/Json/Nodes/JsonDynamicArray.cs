using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;

#nullable enable

namespace System.Text.Json.Nodes;

[DebuggerDisplay("JsonDynamicArray[{_jsonArray.Count}]")]
public class JsonDynamicArray : DynamicObject
{
    private readonly JsonArray _jsonArray;

    public JsonDynamicArray(JsonArray jsonArray) => _jsonArray = jsonArray;

    public object? this[int index]
    {
        get
        {
            if (index >= _jsonArray.Count)
            {
                return null;
            }

            var value = _jsonArray[index];
            if (value is JsonObject jsonObject)
            {
                return new JsonDynamicObject(jsonObject);
            }

            if (value is JsonArray jsonArray)
            {
                return new JsonDynamicArray(jsonArray);
            }

            return value;
        }
    }

    public static implicit operator JsonArray(JsonDynamicArray value) => value._jsonArray;

    public static implicit operator JsonDynamicArray(JsonArray value) => new(value);

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        if (!int.TryParse(binder.Name[1..^1], out var index))
        {
            result = null;
            return false;
        }

        result = this[index];

        return true;
    }

    public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
    {
        result = typeof(JsonArray).InvokeMember(binder.Name, BindingFlags.InvokeMethod, null, _jsonArray, args);
        return true;
    }

    public override IEnumerable<string> GetDynamicMemberNames()
    {
        for (var i = 0; i < _jsonArray.Count; i++)
        {
            yield return $"[{i}]";
        }
    }
}
