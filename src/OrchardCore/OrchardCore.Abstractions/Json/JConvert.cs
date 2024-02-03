using System.Buffers;
using System.Globalization;
using System.Numerics;

namespace System.Text.Json;

#nullable enable

public static class JConvert
{
    /// <summary>
    /// Converts the provided value into a <see cref="string"/>.
    /// </summary>
    public static string SerializeObject<TValue>(TValue value, JsonSerializerOptions? options = null)
        => JsonSerializer.Serialize(value, options ?? JOptions.Default);

    /// <summary>
    /// Parses the text representing a single JSON value into a <typeparamref name="TValue"/>.
    /// </summary>
    public static TValue? DeserializeObject<TValue>(string json, JsonSerializerOptions? options = null)
        => JsonSerializer.Deserialize<TValue>(json, options ?? JOptions.Default);

    /// <summary>
    /// Attempts to read a <see cref="BigInteger"/> value from a <see cref="JsonElement"/>.
    /// </summary>
    public static bool TryGetBigInteger(this JsonElement element, out BigInteger big) =>
        BigInteger.TryParse(element.GetRawText(), NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out big);

    /// <summary>
    /// Attempts to read a <see cref="BigInteger"/> value from a <see cref="Utf8JsonReader"/>.
    /// </summary>
    public static bool TryGetBigInteger(ref Utf8JsonReader reader, out BigInteger big)
    {
        var byteSpan = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
        Span<char> chars = stackalloc char[byteSpan.Length];
        Encoding.UTF8.GetChars(reader.ValueSpan, chars);
        return BigInteger.TryParse(chars, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out big);
    }
}
