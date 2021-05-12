using System.Threading.Tasks;
using OrchardCore.Environment.Shell.Builders.Models;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Environment.Shell.Builders
{
    /// <summary>
    /// Service at the host level to transform the cacheable descriptor into the loadable blueprint.
    /// </summary>
    public interface ICompositionStrategy
    {
        /// <summary>
        /// Using information from the IExtensionManager, transforms and populates all of the
        /// blueprint model the shell builders will need to correctly initialize a tenant IoC container.
        /// </summary>
        Task<ShellBlueprint> ComposeAsync(ShellSettings settings, ShellDescriptor descriptor);
    }
}
