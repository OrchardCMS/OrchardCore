using System;

namespace OrchardCore.Media;

internal static class MediaAppContextSwitches
{
    private const string EnableLegacyMediaFieldGraphQLFieldsKey = "OrchardCore.Media.EnableLegacyMediaFieldGraphQLFields";

    internal static bool EnableLegacyMediaFields => AppContext.TryGetSwitch(EnableLegacyMediaFieldGraphQLFieldsKey, out var enabled) && enabled;
}
