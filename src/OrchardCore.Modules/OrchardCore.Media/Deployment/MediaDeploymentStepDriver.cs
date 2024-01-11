using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Deployment
{
    public class MediaDeploymentStepDriver : DisplayDriver<DeploymentStep, MediaDeploymentStep>
    {
        private readonly IMediaFileStore _mediaFileStore;

        public MediaDeploymentStepDriver(IMediaFileStore mediaFileStore)
        {
            _mediaFileStore = mediaFileStore;
        }

        public override IDisplayResult Display(MediaDeploymentStep step)
        {
            return
                Combine(
                    View("MediaDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("MediaDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(MediaDeploymentStep step)
        {
            return Initialize<MediaDeploymentStepViewModel>("MediaDeploymentStep_Fields_Edit", async model =>
            {
                model.IncludeAll = step.IncludeAll;
                model.FilePaths = step.FilePaths;
                model.DirectoryPaths = step.DirectoryPaths;
                model.Entries = await GetMediaStoreEntries();
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(MediaDeploymentStep step, IUpdateModel updater)
        {
            step.FilePaths = Array.Empty<string>();
            step.DirectoryPaths = Array.Empty<string>();

            await updater.TryUpdateModelAsync(step,
                                              Prefix,
                                              x => x.FilePaths,
                                              x => x.DirectoryPaths,
                                              x => x.IncludeAll);

            // don't have the selected option if include all
            if (step.IncludeAll)
            {
                step.FilePaths = Array.Empty<string>();
                step.DirectoryPaths = Array.Empty<string>();
            }

            return Edit(step);
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
                        : Array.Empty<MediaStoreEntryViewModel>();

                    return mediaStoreEntry;
                }).ToListAsync();

            return mediaStoreEntries;
        }
    }
}
