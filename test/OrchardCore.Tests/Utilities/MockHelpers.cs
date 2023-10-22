using OrchardCore.Settings;

namespace OrchardCore.Tests.Utilities;

public static class MockHelpers
{
    public static ISiteService MockSiteServiceWithSiteSettings(object settings) =>
        Mock.Of<ISiteService>(siteService =>
            siteService.GetSiteSettingsAsync() == Task.FromResult(
                Mock.Of<ISite>(site => site.Properties == Serialize(settings))
            )
        );

    // Necessary to avoid "An expression tree cannot contain a call or invocation that uses optional arguments".
    private static JsonNode Serialize(object settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return JsonSerializer.SerializeToNode(settings);
    }
}
