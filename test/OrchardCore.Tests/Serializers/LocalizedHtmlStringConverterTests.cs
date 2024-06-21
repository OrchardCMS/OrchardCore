using System.Text.Json;

namespace OrchardCore.Tests.Serializers;

public class LocalizedHtmlStringConverterTests
{
    private const string Name = "my text";

    // JOptions.Default already contains the LocalizedHtmlStringConverter being tested here.
    private static readonly JsonSerializerOptions _options = JOptions.Default;

    [Theory]
    [InlineData(Name, Name, null, "{\"Name\":\"my text\",\"Value\":\"my text\",\"IsResourceNotFound\":false}")]
    [InlineData(Name, Name, false, "{\"Name\":\"my text\",\"Value\":\"my text\",\"IsResourceNotFound\":false}")]
    [InlineData(Name, Name, true, "{\"Name\":\"my text\",\"Value\":\"my text\",\"IsResourceNotFound\":true}")]
    [InlineData(
        Name,
        "az én szövegem", // Localized to Hungarian, just to test serialization with non-ASCII characters.
        null,
        "{\"Name\":\"my text\",\"Value\":\"az \\u00E9n sz\\u00F6vegem\",\"IsResourceNotFound\":false}")]
    public void LocalizedHtmlStringShouldBeSerializedCorrectly(string name, string value, bool? notFound, string expected)
    {
        var localized = notFound == null ? new LocalizedHtmlString(name, value) : new(name, value, notFound.Value);

        Assert.Equal(expected, JsonSerializer.Serialize(localized, _options));
    }

    [Theory]
    [InlineData("\"my text\"", Name, Name, false)]
    [InlineData("{ \"name\": \"my text\", \"value\": \"my text\", \"isResourceNotFound\": true }", Name, Name, true)]
    [InlineData("{ \"name\": \"my text\", \"value\": \"some other text\" }", Name, "some other text", false)]
    [InlineData("{ \"value\": \"my text\" }", Name, Name, false)]
    [InlineData("{ \"NAME\": \"my text\" }", Name, Name, false)]
    public void LocalizedHtmlStringShouldBeDeserializedCorrectly(string json, string name, string value, bool notFound)
    {
        var localized = JsonSerializer.Deserialize<LocalizedHtmlString>(json, _options);

        Assert.Equal(name, localized.Name);
        Assert.Equal(value, localized.Value);
        Assert.Equal(notFound, localized.IsResourceNotFound);
    }
}

