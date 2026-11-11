using Microsoft.Extensions.Options;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using OrchardCore.Jobs;

namespace OrchardCore.Indexing.Jobs;

/// <summary>
/// Updates the content indexes that all the servers share, such as the ones of a remote search service, with the
/// content items that changed. A single server does it at a time, whatever the number of servers. The indexes each
/// server keeps for itself, such as on its disk, are updated by the indexing background task of each server instead.
/// </summary>
public sealed class ContentIndexingJob : IJob
{
    /// <summary>
    /// The name of the job. It is also the key that prevents two of its runs from being executed at the same time.
    /// </summary>
    public const string Name = "ContentIndexing";

    private readonly IIndexProfileStore _indexProfileStore;
    private readonly ContentIndexingService _indexingService;
    private readonly IndexingOptions _indexingOptions;

    public ContentIndexingJob(
        IIndexProfileStore indexProfileStore,
        ContentIndexingService indexingService,
        IOptions<IndexingOptions> indexingOptions)
    {
        _indexProfileStore = indexProfileStore;
        _indexingService = indexingService;
        _indexingOptions = indexingOptions.Value;
    }

    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        var indexProfiles = await GetIndexProfilesAsync(_indexProfileStore, _indexingOptions, local: false);

        if (indexProfiles.Count == 0)
        {
            context.LogInformation("There is no content index shared by the servers.");

            return;
        }

        context.ReportProgress(null, $"Updating {indexProfiles.Count} content index(es).");

        await _indexingService.ProcessRecordsAsync(indexProfiles.Select(indexProfile => indexProfile.Id).ToArray());

        context.LogInformation($"The content index(es) {string.Join(", ", indexProfiles.Select(indexProfile => indexProfile.Name))} were updated.");
    }

    /// <summary>
    /// Returns the content indexes each server keeps for itself, or the ones the servers share.
    /// </summary>
    internal static async Task<List<IndexProfile>> GetIndexProfilesAsync(IIndexProfileStore indexProfileStore, IndexingOptions indexingOptions, bool local)
    {
        var indexProfiles = await indexProfileStore.GetByTypeAsync(IndexingConstants.ContentsIndexSource);

        return indexProfiles
            .Where(indexProfile => indexingOptions.IsLocalProvider(indexProfile.ProviderName) == local)
            .ToList();
    }
}
