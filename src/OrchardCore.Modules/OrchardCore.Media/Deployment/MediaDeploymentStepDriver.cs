using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Deployment;

public sealed class MediaDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<MediaDeploymentStep, MediaDeploymentStepViewModel>
{
    private readonly IMediaFileStore _mediaFileStore;

    public MediaDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _mediaFileStore = serviceProvider.GetService<IMediaFileStore>();
    }

    public override IDisplayResult Edit(MediaDeploymentStep step, Action<MediaDeploymentStepViewModel> intializeAction)
    {
        return base.Edit(step, async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.FilePaths = step.FilePaths;
            model.DirectoryPaths = step.DirectoryPaths;
            model.Entries = await GetMediaStoreEntries();
        });
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

    private async Task<IList<MediaStoreEntryViewModel>> GetMediaStoreEntries(string path = null, MediaStoreEntryViewModel parent = null)
    {
        var mediaStoreEntries = await _mediaFileStore.GetDirectoryContentAsync(path)
            .SelectAwait(async e =>
            {
                var mediaStoreEntry = new MediaStoreEntryViewModel
                {
                    Name = e.Name,
                    Path = e.Path,
                    Parent = parent
                };

                mediaStoreEntry.Entries = e.IsDirectory
                    ? await GetMediaStoreEntries(e.Path, mediaStoreEntry)
                    : [];

                return mediaStoreEntry;
            }).ToListAsync();

        return mediaStoreEntries;
    }
}
