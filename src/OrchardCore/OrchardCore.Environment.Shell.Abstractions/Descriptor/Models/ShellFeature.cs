namespace OrchardCore.Environment.Shell.Descriptor.Models
{
    public class ShellFeature
    {
        public ShellFeature()
        {
        }

        public ShellFeature(string id, bool alwaysEnabled = false)
        {
            Id = id;
            AlwaysEnabled = alwaysEnabled;
        }

        public string Id { get; set; }
        public bool AlwaysEnabled { get; set; }
    }
}
