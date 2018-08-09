namespace OrchardCore.Environment.Shell.Descriptor.Models
{
    /// <summary>
    /// A shell parameter is a custom value that can be assigned to a specific component in a shell.
    /// </summary>
    public class ShellParameter
    {
        public string Component { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
