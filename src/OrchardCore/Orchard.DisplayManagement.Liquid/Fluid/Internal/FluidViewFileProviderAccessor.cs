using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Orchard.DisplayManagement.Fluid.Internal
{
    /// <summary>
    /// Default implementation of <see cref="IFluidViewFileProviderAccessor"/>.
    /// </summary>
    public class FluidViewFileProviderAccessor : IFluidViewFileProviderAccessor
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FluidViewFileProviderAccessor"/>.
        /// </summary>
        /// <param name="optionsAccessor">Accessor to <see cref="FluidViewOptions"/>.</param>
        public FluidViewFileProviderAccessor(IOptions<FluidViewOptions> optionsAccessor)
        {
            var fileProviders = optionsAccessor.Value.FileProviders;

            if (fileProviders.Count == 0)
            {
                FileProvider = new NullFileProvider();
            }
            else if (fileProviders.Count == 1)
            {
                FileProvider = fileProviders[0];
            }
            else
            {
                FileProvider = new CompositeFileProvider(fileProviders);
            }
        }

        /// <summary>
        /// Gets the <see cref="IFileProvider"/> used to look up Fluid files.
        /// </summary>
        public IFileProvider FileProvider { get; }
    }
}