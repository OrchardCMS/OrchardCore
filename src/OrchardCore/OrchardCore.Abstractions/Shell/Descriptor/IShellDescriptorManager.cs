using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Environment.Shell.Descriptor
{
    /// <summary>
    /// Service resolved out of the shell container. Primarily used by host.
    /// </summary>
    public interface IShellDescriptorManager
    {
        /// <summary>
        /// Uses shell-specific database or other resources to return
        /// the current "correct" configuration. The host will use this information
        /// to reinitialize the shell.
        /// </summary>
        Task<ShellDescriptor> GetShellDescriptorAsync();

        /// <summary>
        /// Alters databased information to match information passed as arguments.
        /// Prior SerialNumber used for optimistic concurrency, and an exception
        /// should be thrown if the number in storage doesn't match what's provided.
        /// </summary>
        Task UpdateShellDescriptorAsync(int priorSerialNumber, IEnumerable<ShellFeature> enabledFeatures);
    }
}
