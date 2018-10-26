namespace OrchardCore.Media.ViewModels
{
    public class MediaStoreEntryViewModel
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public MediaStoreEntryViewModel[] Entries { get; set; }
    }
}