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
}
