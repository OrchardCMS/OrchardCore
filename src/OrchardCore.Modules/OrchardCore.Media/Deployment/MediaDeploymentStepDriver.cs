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
            return Initialize<MediaDeploymentStepViewModel>("MediaDeploymentStep_Fields_Edit", model =>
            {
                model.IncludeAll = step.IncludeAll;
                model.Paths = step.Paths;
                model.Entries = GetMediaStoreEntries().ToArray();
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(MediaDeploymentStep step, IUpdateModel updater)
        {
            step.Paths = Array.Empty<string>();

            await updater.TryUpdateModelAsync(step,
                                              Prefix,
                                              x => x.Paths,
                                              x => x.IncludeAll);

            // don't have the selected option if include all
            if (step.IncludeAll)
            {
                step.Paths = Array.Empty<string>();
            }

            return Edit(step);
        }

        private IEnumerable<MediaStoreEntryViewModel> GetMediaStoreEntries(string path = null)
        {
            var fileStoreEntries = _mediaFileStore.GetDirectoryContentAsync(path).Result;

            foreach (var fileStoreEntry in fileStoreEntries)
            {
                yield return new MediaStoreEntryViewModel
                {
                    Name = fileStoreEntry.Name,
                    Path = fileStoreEntry.Path,
                    Entries = fileStoreEntry.IsDirectory
                        ? GetMediaStoreEntries(fileStoreEntry.Path).ToArray()
                        : Array.Empty<MediaStoreEntryViewModel>()
                };
            }
        }
    }
}