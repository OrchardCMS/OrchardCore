using OrchardCore.Localization;

namespace OrchardCore.Microsoft.Authentication;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Microsoft.Authentication.MicrosoftAuthenticationLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class MicrosoftAuthenticationLocalizationSources
{
    /// <summary>Gets the "Microsoft Authentication" text.</summary>
    public static readonly LocalizationSource MicrosoftAuthentication = LocalizationSource.Create("Microsoft Authentication", typeof(MicrosoftAuthenticationLocalizationSources));
}
