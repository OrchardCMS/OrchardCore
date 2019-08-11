namespace OrchardCore.ContentTypes.ViewModels
{
    public class ContentDefinitionStepViewModel
    {
        public string[] ContentTypes { get; set; }

        public string[] ContentParts { get; set; }

        public bool IncludeAll { get; internal set; }
    }
}
