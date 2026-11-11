using OrchardCore.Localization;

namespace OrchardCore.AuditTrail;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.AuditTrail.AuditTrailLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class AuditTrailLocalizationSources
{
    /// <summary>Gets the title of the trimming job.</summary>
    public static readonly LocalizationSource TrimmingJobTitle = LocalizationSource.Create("Audit Trail Trimming", typeof(AuditTrailLocalizationSources));

    /// <summary>Gets the description of the trimming job.</summary>
    public static readonly LocalizationSource TrimmingJobDescription = LocalizationSource.Create("Deletes the audit trail events older than the retention period of the audit trail settings.", typeof(AuditTrailLocalizationSources));
}
