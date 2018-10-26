namespace OrchardCore.Media.ViewModels
{
    public class MediaDeploymentStepViewModel
    {
        public bool IncludeAll { get; set; }

        public string[] Paths { get; set; }

        public MediaStoreEntryViewModel[] Entries { get; set; }
    }
}