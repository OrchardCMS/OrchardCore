using System.Text.Json;
using OrchardCore.Documents;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Serializers;

public class DefaultDocumentSerializerTests
{
    [Fact]
    public async Task ShouldSerializeAndDeserialize()
    {
        var settings = new SiteSettings
        {
            AppendVersion = true,
            BaseUrl = "http://localhost",
        };

        var serializer = new DefaultDocumentSerializer(JsonSerializerOptions.Default);

        var data = await serializer.SerializeAsync(settings, 0);

        // Data should be gzipped
        Assert.Equal([0x1f, 0x8b], data.AsSpan().Slice(0, 2).ToArray());

        var settings2 = await serializer.DeserializeAsync<SiteSettings>(data);

        Assert.Equal(settings.AppendVersion, settings2.AppendVersion);
        Assert.Equal(settings.BaseUrl, settings2.BaseUrl);
    }
}
