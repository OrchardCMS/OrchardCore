using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.Events;
using System.Threading.Tasks;

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
        Task Changed(ShellDescriptor descriptor, string tenant);
    }
}
