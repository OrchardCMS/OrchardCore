using System.Collections.Generic;

namespace OrchardCore.Media.ViewModels
{
    public class MediaDeploymentStepViewModel
    {
        public bool IncludeAll { get; set; } = true;

        public string[] FilePaths { get; set; }

        public string[] DirectoryPaths { get; set; }

        public MediaStoreEntryViewModel ParentEntry { get; set; }

        public IList<MediaStoreEntryViewModel> Entries { get; set; }
    }
}
