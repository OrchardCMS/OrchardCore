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

    /// <summary>Gets the title of the job that executes a run of a data pipeline.</summary>
    public static readonly LocalizationSource RunJobTitle = LocalizationSource.Create("Data Pipeline", typeof(DataPipelinesLocalizationSources));

    /// <summary>Gets the description of the job that executes a run of a data pipeline.</summary>
    public static readonly LocalizationSource RunJobDescription = LocalizationSource.Create("Executes a run of a data pipeline. The run history of the data pipelines shows what each run did.", typeof(DataPipelinesLocalizationSources));

    /// <summary>Gets the title of the job that deletes the old runs of the data pipelines.</summary>
    public static readonly LocalizationSource RunsCleanupJobTitle = LocalizationSource.Create("Data Pipeline Runs Cleanup", typeof(DataPipelinesLocalizationSources));

    /// <summary>Gets the description of the job that deletes the old runs of the data pipelines.</summary>
    public static readonly LocalizationSource RunsCleanupJobDescription = LocalizationSource.Create("Deletes the old runs of the data pipelines, and queues again the runs that no job executes.", typeof(DataPipelinesLocalizationSources));

    /// <summary>Gets the title of the job that deletes the shared links that stopped working.</summary>
    public static readonly LocalizationSource SharedLinksCleanupJobTitle = LocalizationSource.Create("Shared Links Cleanup", typeof(DataPipelinesLocalizationSources));

    /// <summary>Gets the description of the job that deletes the shared links that stopped working.</summary>
    public static readonly LocalizationSource SharedLinksCleanupJobDescription = LocalizationSource.Create("Deletes the shared links that stopped working, and their resources.", typeof(DataPipelinesLocalizationSources));
}
