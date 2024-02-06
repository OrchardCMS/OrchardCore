namespace OrchardCore.Environment.Shell.Descriptor.Models;

public class InstalledShellFeature : ShellFeature
{
    public InstalledShellFeature()
    {
    }

    public InstalledShellFeature(ShellFeature feature)
    {
        Id = feature.Id;
        AlwaysEnabled = feature.AlwaysEnabled;
    }

    /// <summary>
    /// The version number of the descriptor when the feature was installed.
    /// </summary>
    public int SerialNumber { get; set; }
}
