using System.Threading.Tasks;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// Represent an event handler for shell descriptor.
    /// </summary>
    public interface IShellDescriptorManagerEventHandler
    {
        /// <summary>
        /// Triggered whenever a shell descriptor has changed.
        /// </summary>
        Task ChangedAsync(ShellDescriptor descriptor, ShellSettings settings);
    }
}
