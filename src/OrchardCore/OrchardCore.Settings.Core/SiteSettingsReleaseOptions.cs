namespace OrchardCore.Settings;

/// <summary>
/// Lists the site settings that are only read when the tenant is built, so that the tenant is released
/// when a recipe updates them. Other site settings are expected to be read when they are used, or to refresh
/// the options built from them through <see cref="Environment.Options.IOptionsUpdateNotifier"/>.
/// </summary>
public sealed class SiteSettingsReleaseOptions
{
    /// <summary>
    /// Gets the names of the site settings, i.e. their property names in the site settings,
    /// whose update requires the tenant to be released.
    /// </summary>
    public HashSet<string> Settings { get; } = new(StringComparer.OrdinalIgnoreCase);
}
