namespace OrchardCore.Media.Services;

internal static class MediaApiSettingsEditor
{
    public static bool IsValid(MediaApiAuthenticationScheme scheme) => Enum.IsDefined(scheme);

    public static bool Apply(MediaApiSettings settings, MediaApiAuthenticationScheme scheme)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(IsValid(scheme), true, nameof(scheme));
        if (settings.AuthenticationScheme == scheme) { return false; }
        settings.AuthenticationScheme = scheme;
        return true;
    }
}
