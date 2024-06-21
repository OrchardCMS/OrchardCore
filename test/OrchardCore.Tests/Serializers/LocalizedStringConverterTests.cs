using System.Text.Json;

namespace OrchardCore.Tests.Serializers;

public class LocalizedStringConverterTests
{
    private const string Name = "my text";
    private const string Location = "/some/location.po";

    // JOptions.Default already contains the LocalizedStringConverter being tested here.
    private static readonly JsonSerializerOptions _options = JOptions.Default;

    [Theory]
    [InlineData(Name, Name, false, null, "{\"Name\":\"my text\",\"Value\":\"my text\",\"ResourceNotFound\":false,\"SearchedLocation\":null}")]
    [InlineData(Name, Name, false, Location, "{\"Name\":\"my text\",\"Value\":\"my text\",\"ResourceNotFound\":false,\"SearchedLocation\":\"/some/location.po\"}")]
    [InlineData(Name, Name, true, null, "{\"Name\":\"my text\",\"Value\":\"my text\",\"ResourceNotFound\":true,\"SearchedLocation\":null}")]
    [InlineData(
        Name,
        "az én szövegem", // Localized to Hungarian, just to test serialization with non-ASCII characters.
        false,
        null,
        "{\"Name\":\"my text\",\"Value\":\"az \\u00E9n sz\\u00F6vegem\",\"ResourceNotFound\":false,\"SearchedLocation\":null}")]
    public void LocalizedStringShouldBeSerializedCorrectly(string name, string value, bool notFound, string location, string expected)
    {
        var localized = new LocalizedString(name, value, notFound, location);

        Assert.Equal(expected, JsonSerializer.Serialize(localized, _options));
    }

    [Theory]
    [InlineData("\"my text\"", Name, Name, false, null)]
    [InlineData(
        "{ \"name\": \"my text\", \"value\": \"my text\", \"resourceNotFound\": true, \"SearchedLocation\": \"/some/location.po\" }",
        Name,
        Name,
        true,
        Location)]
    [InlineData("{ \"name\": \"my text\", \"value\": \"some other text\" }", Name, "some other text", false, null)]
    [InlineData("{ \"value\": \"my text\" }", Name, Name, false, null)]
    [InlineData("{ \"NAME\": \"my text\" }", Name, Name, false, null)]
    public void LocalizedStringShouldBeDeserializedCorrectly(string json, string name, string value, bool notFound, string location)
    {
        var localized = JsonSerializer.Deserialize<LocalizedString>(json, _options);

        Assert.Equal(name, localized.Name);
        Assert.Equal(value, localized.Value);
        Assert.Equal(notFound, localized.ResourceNotFound);
        Assert.Equal(location, localized.SearchedLocation);
    }
}


