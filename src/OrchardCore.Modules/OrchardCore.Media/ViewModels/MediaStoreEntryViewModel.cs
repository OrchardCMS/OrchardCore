using System.Collections.Generic;

namespace OrchardCore.Media.ViewModels
{
    public class MediaStoreEntryViewModel
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public MediaStoreEntryViewModel Parent { get; set; }

        public IList<MediaStoreEntryViewModel> Entries { get; set; }
    }
}
