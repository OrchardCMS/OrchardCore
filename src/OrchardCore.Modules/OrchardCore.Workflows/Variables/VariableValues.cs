using System.Collections;
using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OrchardCore.Workflows.Variables;

/// <summary>
/// Conversions shared by the built-in variable types.
/// </summary>
internal static class VariableValues
{
    /// <summary>
    /// Returns a JSON node as the CLR value the workflow engine reads after a save and reload: a string, a
    /// boolean, a number, a <see cref="Dictionary{TKey, TValue}"/> or a <see cref="List{T}"/>.
    /// </summary>
    public static object FromJson(JsonNode node)
        => node switch
        {
            null => null,
            JsonObject jsonObject => jsonObject.Deserialize<Dictionary<string, object>>(JOptions.Default),
            JsonArray jsonArray => jsonArray.Deserialize<List<object>>(JOptions.Default),
            JsonValue jsonValue => jsonValue.GetValueKind() switch
            {
                JsonValueKind.String => jsonValue.Deserialize<string>(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number => jsonValue.Deserialize<double>(),
                _ => null,
            },
            _ => null,
        };

    public static bool IsNumber(object value)
        => value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal or BigInteger;

    public static double ToDouble(object value)
        => value is BigInteger bigInteger ? (double)bigInteger : Convert.ToDouble(value, CultureInfo.InvariantCulture);

    // A collection that isn't a string or a dictionary.
    public static bool IsList(object value)
        => value is IEnumerable and not string and not IDictionary and not IDictionary<string, object> and not JsonNode;
}
