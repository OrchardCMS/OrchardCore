using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Environment.Shell;

public static class ShellDescriptorExtensions
{
    /// <summary>
    /// Checks if the specified feature was already installed.
    /// </summary>
    public static bool WasFeatureAlreadyInstalled(this ShellDescriptor shellDescriptor, string featureId)
    {
        var installed = shellDescriptor.Installed.FirstOrDefault(feature => feature.Id == featureId);
        if (installed is null)
        {
            return false;
        }

        return installed.SerialNumber != shellDescriptor.SerialNumber;
    }
}
