using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy
{
    /// <summary>
    /// Default implementation of <see cref="IShapeTemplateFileProviderAccessor"/>.
    /// </summary>
    public class ShapeTemplateFileProviderAccessor : IShapeTemplateFileProviderAccessor
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ShapeTemplateFileProviderAccessor"/>.
        /// </summary>
        /// <param name="optionsAccessor">Accessor to <see cref="ShapeTemplateOptions"/>.</param>
        public ShapeTemplateFileProviderAccessor(IOptions<ShapeTemplateOptions> optionsAccessor)
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
        /// Gets the <see cref="IFileProvider"/> used to look up Templates files.
        /// </summary>
        public IFileProvider FileProvider { get; }
    }
}
