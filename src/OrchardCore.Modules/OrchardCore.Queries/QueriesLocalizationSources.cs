using OrchardCore.Localization;

namespace OrchardCore.Queries;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Queries.QueriesLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class QueriesLocalizationSources
{
    /// <summary>Gets the "Queried Content Items" text.</summary>
    public static readonly LocalizationSource QueriedContentItems = LocalizationSource.Create("Queried Content Items", typeof(QueriesLocalizationSources));
}
