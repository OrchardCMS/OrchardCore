using OrchardCore.Localization;

namespace OrchardCore.Indexing;

/// <summary>
/// Untranslated strings of the jobs of this module. Their PO context is <c>OrchardCore.Indexing.IndexingJobsLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class IndexingJobsLocalizationSources
{
    /// <summary>Gets the title of the job that updates the shared content indexes.</summary>
    public static readonly LocalizationSource ContentIndexingJobTitle = LocalizationSource.Create("Content Indexing", typeof(IndexingJobsLocalizationSources));

    /// <summary>Gets the description of the job that updates the shared content indexes.</summary>
    public static readonly LocalizationSource ContentIndexingJobDescription = LocalizationSource.Create("Updates the content indexes that all the servers share, such as the ones of a remote search service, with the content items that changed.", typeof(IndexingJobsLocalizationSources));

    /// <summary>Gets the title of the job that synchronizes an index.</summary>
    public static readonly LocalizationSource SynchronizeJobTitle = LocalizationSource.Create("Index Synchronization", typeof(IndexingJobsLocalizationSources));

    /// <summary>Gets the description of the job that synchronizes an index.</summary>
    public static readonly LocalizationSource SynchronizeJobDescription = LocalizationSource.Create("Synchronizes an index with its source, such as after it was reset or rebuilt.", typeof(IndexingJobsLocalizationSources));
}
