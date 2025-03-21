using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Deployment;

public sealed class MediaDeploymentStepDriver : DisplayDriver<DeploymentStep, MediaDeploymentStep>
{
    private readonly IMediaFileStore _mediaFileStore;

    public MediaDeploymentStepDriver(IMediaFileStore mediaFileStore)
    {
        _mediaFileStore = mediaFileStore;
    }

    public override Task<IDisplayResult> DisplayAsync(MediaDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("MediaDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("MediaDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(MediaDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<MediaDeploymentStepViewModel>("MediaDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.FilePaths = step.FilePaths;
            model.DirectoryPaths = step.DirectoryPaths;
            model.Entries = await GetMediaStoreEntries().ToListAsync();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(MediaDeploymentStep step, UpdateEditorContext context)
    {
        step.FilePaths = [];
        step.DirectoryPaths = [];

        await context.Updater.TryUpdateModelAsync(step,
                                          Prefix,
                                          x => x.FilePaths,
                                          x => x.DirectoryPaths,
                                          x => x.IncludeAll);

        // Don't have the selected option if include all.
        if (step.IncludeAll)
        {
            step.FilePaths = [];
            step.DirectoryPaths = [];
        }

        return Edit(step, context);
    }

    private async IAsyncEnumerable<MediaStoreEntryViewModel> GetMediaStoreEntries(string path = null, MediaStoreEntryViewModel parent = null)
    {
        await foreach (var e in _mediaFileStore.GetDirectoryContentAsync(path))
        {
            var mediaStoreEntry = new MediaStoreEntryViewModel
            {
                Name = e.Name,
                Path = e.Path,
                Parent = parent
            };

            mediaStoreEntry.Entries = e.IsDirectory
                ? await GetMediaStoreEntries(e.Path, mediaStoreEntry).ToListAsync()
                : [];

            yield return mediaStoreEntry;
        }
    }
}
