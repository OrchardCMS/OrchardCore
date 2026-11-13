using OrchardCore.Contents.VersionPruning.Models;
using OrchardCore.Contents.VersionPruning.Services;
using OrchardCore.Jobs;
using OrchardCore.Settings;

namespace OrchardCore.Contents.VersionPruning.Tasks;

/// <summary>
/// Deletes the old versions of content items that are neither the latest nor the published one, as the content
/// version pruning settings tell. Its schedule runs it every day, once whatever the number of servers, and is
/// disabled by default.
/// </summary>
public sealed class ContentVersionPruningJob : IJob
{
    /// <summary>
    /// The name of the job.
    /// </summary>
    public const string Name = "ContentVersionPruning";

    private readonly ISiteService _siteService;
    private readonly IContentVersionPruningService _pruningService;

    public ContentVersionPruningJob(ISiteService siteService, IContentVersionPruningService pruningService)
    {
        _siteService = siteService;
        _pruningService = pruningService;
    }

    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        var settings = await _siteService.GetSettingsAsync<ContentVersionPruningSettings>();

        if (settings.Disabled)
        {
            context.LogInformation("The pruning is disabled in the content version pruning settings.");

            return;
        }

        var pruned = await _pruningService.PruneVersionsAsync(settings, cancellationToken);

        context.LogInformation($"{pruned} content item versions were deleted.");
        context.SetResult(new { DeletedVersions = pruned });
    }
}
