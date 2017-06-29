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
    }
}