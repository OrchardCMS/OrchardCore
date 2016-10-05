namespace Orchard.Environment.Shell.Descriptor.Models
{
    public class ShellFeature
    {
        public ShellFeature()
        {
        }

        public ShellFeature(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
