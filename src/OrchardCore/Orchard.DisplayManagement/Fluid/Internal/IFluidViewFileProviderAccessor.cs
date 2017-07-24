using Microsoft.Extensions.FileProviders;

namespace Orchard.DisplayManagement.Fluid.Internal
{
    /// <summary>
    /// Accessor to the <see cref="IFileProvider"/> used by <see cref="FluidViewTemplate"/>.
    /// </summary>
    public interface IFluidViewFileProviderAccessor
    {
        /// <summary>
        /// Gets the <see cref="IFileProvider"/> used to look up Fluid files.
        /// </summary>
        IFileProvider FileProvider { get; }

        /// <summary>
        /// Gets the <see cref="IFileProvider"/> used to look up Fluid files across all tenants.
        /// </summary>
        IFileProvider SharedFileProvider { get; }

        /// <summary>
        /// Gets the <see cref="IFileProvider"/> used to look up Fluid files for a given tenant.
        /// </summary>
        IFileProvider ShellFileProvider { get; }
    }
}