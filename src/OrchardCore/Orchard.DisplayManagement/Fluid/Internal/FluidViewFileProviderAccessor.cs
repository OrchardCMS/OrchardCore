using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.FileProviders;

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
            var sharedFileProviders = fileProviders.Where(fp => !(fp is IShellFileProvider)).ToList();
            var shellFileProviders = fileProviders.Where(fp => fp is IShellFileProvider).ToList();

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

            if (sharedFileProviders.Count == 0)
            {
                SharedFileProvider = new NullFileProvider();
            }
            else if (sharedFileProviders.Count == 1)
            {
                SharedFileProvider = sharedFileProviders[0];
            }
            else
            {
                SharedFileProvider = new CompositeFileProvider(sharedFileProviders);
            }

            if (shellFileProviders.Count == 0)
            {
                ShellFileProvider = new NullFileProvider();
            }
            else if (shellFileProviders.Count == 1)
            {
                ShellFileProvider = shellFileProviders[0];
            }
            else
            {
                ShellFileProvider = new CompositeFileProvider(shellFileProviders);
            }
        }

        /// <summary>
        /// Gets the <see cref="IFileProvider"/> used to look up Fluid files.
        /// </summary>
        public IFileProvider FileProvider { get; }

        /// <summary>
        /// Gets the <see cref="IFileProvider"/> used to look up Fluid files across all tenants.
        /// </summary>
        public IFileProvider SharedFileProvider { get; }

        /// <summary>
        /// Gets the <see cref="IFileProvider"/> used to look up Fluid files for a given tenant.
        /// </summary>
        public IFileProvider ShellFileProvider { get; }
    }
}