using OrchardCore.Localization;

namespace OrchardCore.DataPipelines;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.DataPipelines.DataPipelinesLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class DataPipelinesLocalizationSources
{
    /// <summary>Gets the "Data" text.</summary>
    public static readonly LocalizationSource Data = LocalizationSource.Create("Data", typeof(DataPipelinesLocalizationSources));

    /// <summary>Gets the "Data Pipelines" text.</summary>
    public static readonly LocalizationSource DataPipelines = LocalizationSource.Create("Data Pipelines", typeof(DataPipelinesLocalizationSources));
}
