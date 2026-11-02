using OrchardCore.Localization;

namespace OrchardCore.Tenants;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Tenants.TenantsLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class TenantsLocalizationSources
{
    /// <summary>Gets the "Tenant" activity category.</summary>
    public static readonly LocalizationSource Tenant = LocalizationSource.Create("Tenant", typeof(TenantsLocalizationSources));
}
