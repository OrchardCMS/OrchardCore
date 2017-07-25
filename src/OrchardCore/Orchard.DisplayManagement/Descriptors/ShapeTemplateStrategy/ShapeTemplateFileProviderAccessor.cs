using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.FileProviders;

namespace Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy
{
    /// <summary>
    /// Default implementation of <see cref="ITemplateFileProviderAccessor"/>.
    /// </summary>
    public class ShapeTemplateFileProviderAccessor : IShapeTemplateFileProviderAccessor
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ShapeTemplateFileProviderAccessor"/>.
        /// </summary>
        /// <param name="optionsAccessor">Accessor to <see cref="FluidViewOptions"/>.</param>
        public ShapeTemplateFileProviderAccessor(IOptions<ShapeTemplateOptions> optionsAccessor)
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
        /// Gets the <see cref="IFileProvider"/> used to look up Templates files.
        /// </summary>
        public IFileProvider FileProvider { get; }

        /// <summary>
        /// Gets the <see cref="IFileProvider"/> used to look up Templates files across all tenants.
        /// </summary>
        public IFileProvider SharedFileProvider { get; }

        /// <summary>
        /// Gets the <see cref="IFileProvider"/> used to look up Templates files for a given tenant.
        /// </summary>
        public IFileProvider ShellFileProvider { get; }
    }
}