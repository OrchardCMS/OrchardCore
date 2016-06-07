using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.Events;

namespace Orchard.Environment.Shell
{
    /// <summary>
    /// Represent and event handler for shell descriptor.
    /// </summary>
    public interface IShellDescriptorManagerEventHandler : IEventHandler
    {
        /// <summary>
        /// Triggered whenever a shell descriptor has changed.
        /// </summary>
        void Changed(ShellDescriptor descriptor, string tenant);
    }
}
