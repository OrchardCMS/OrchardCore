namespace OrchardCore.Media;

// Leave this type and the const internal so that we can remove it at any time
// in future releases.
internal static class MediaAppContextSwitches
{
    private const string EnableLegacyMediaFieldGraphQLFieldsKey = "OrchardCore.Media.EnableLegacyMediaFieldGraphQLFields";

    internal static bool EnableLegacyMediaFields => AppContext.TryGetSwitch(EnableLegacyMediaFieldGraphQLFieldsKey, out var enabled) && enabled;
}
