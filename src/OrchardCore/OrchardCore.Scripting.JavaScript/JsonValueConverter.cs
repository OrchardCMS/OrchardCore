using System.Text.Json;
using System.Text.Json.Nodes;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;

namespace OrchardCore.Scripting.JavaScript;
public class JsonValueConverter : IObjectConverter
{
    public bool TryConvert(Engine engine, object value, out JsValue result)
    {
        if (value is JsonValue jsonValue)
        {
            var valueKind = jsonValue.GetValueKind();
            switch (valueKind)
            {
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    result = JsValue.FromObject(engine, jsonValue);
                    break;
                case JsonValueKind.String:
                    result = jsonValue.ToString();
                    break;
                case JsonValueKind.Number:
                    if (jsonValue.TryGetValue<double>(out var doubleValue))
                    {
                        result = JsNumber.Create(doubleValue);
                    }
                    else
                    {
                        result = JsValue.Undefined;
                    }
                    break;
                case JsonValueKind.True:
                    result = JsBoolean.True;
                    break;
                case JsonValueKind.False:
                    result = JsBoolean.False;
                    break;
                case JsonValueKind.Undefined:
                    result = JsValue.Undefined;
                    break;
                case JsonValueKind.Null:
                    result = JsValue.Null;
                    break;
                default:
                    result = JsValue.Undefined;
                    break;
            }
            return true;
        }
        result = JsValue.Undefined;
        return false;

    }
}
