using OrchardCore.Localization;

namespace OrchardCore.OpenId;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.OpenId.OpenIdLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class OpenIdLocalizationSources
{
    /// <summary>Gets the "OpenID Connect" text.</summary>
    public static readonly LocalizationSource OpenIdConnect = LocalizationSource.Create("OpenID Connect", typeof(OpenIdLocalizationSources));
}
