namespace OrchardCore.Media.Azure.ViewModels
{
    public class OptionsViewModel
    {
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
        public string BasePath { get; set; }
        public bool CreateContainer { get; set; }
    }
}
