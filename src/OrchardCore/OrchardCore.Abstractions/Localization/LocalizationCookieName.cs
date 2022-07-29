using OrchardCore.Environment.Shell;

namespace OrchardCore.Localization;

/// <summary>
/// Localization cookie name constants.
/// </summary>
public static class LocalizationCookieName
{
    /// <summary>
    /// The prefix of the admin culture cookie name, at runtime the final
    /// name will be suffixed by the <see cref="ShellSettings.VersionId"/>.
    /// </summary>
    public const string AdminCulturePrefix = "admin_culture_";
}
