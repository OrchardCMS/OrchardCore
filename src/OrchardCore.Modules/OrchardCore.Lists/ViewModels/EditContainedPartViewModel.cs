namespace OrchardCore.Lists.ViewModels
{
    public class EditContainedPartViewModel
    {
        public string ContainerId { get; set; }
        public string ContainerContentType { get; set; }
        public string ContentType { get; set; }
        public bool EnableOrdering { get; set; }
    }
}
