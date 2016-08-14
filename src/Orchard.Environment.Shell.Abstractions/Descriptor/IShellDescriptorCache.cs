using Orchard.Environment.Shell.Descriptor.Models;

namespace Orchard.Environment.Shell.Descriptor
{
    /// <summary>
    /// Single service instance registered at the host level. Provides storage
    /// and recall of shell descriptor information. Default implementation uses
    /// app_data, but configured replacements could use other per-host writable location.
    /// </summary>
    public interface IShellDescriptorCache
    {
        /// <summary>
        /// Recreate the named configuration information. Used at startup. 
        /// Returns null on cache-miss.
        /// </summary>
        ShellDescriptor Fetch(string shellName);

        /// <summary>
        /// Commit named configuration to reasonable persistent storage.
        /// This storage is scoped to the current-server and current-webapp.
        /// Loss of storage is expected.
        /// </summary>
        void Store(string shellName, ShellDescriptor descriptor);
    }
}
