using System.Text.Json.Nodes;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Utilities;

public static class SiteMockHelper
{
    public static Mock<ISite> GetSite<T>(T obj) where T : new()
    {
        var properties = new JsonObject
        {
            [obj.GetType().Name] = JObject.FromObject(obj),
        };

        var mockSite = new Mock<ISite>();
        mockSite.Setup(x => x.Properties)
            .Returns(properties);

        mockSite.Setup(x => x.GetOrCreate<T>())
            .Returns(obj);

        mockSite.Setup(x => x.TryGet(out obj))
            .Returns(true);

        return mockSite;
    }
}
