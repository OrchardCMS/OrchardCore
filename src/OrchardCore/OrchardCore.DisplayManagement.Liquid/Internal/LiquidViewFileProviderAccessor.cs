using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace OrchardCore.DisplayManagement.Liquid.Internal
{
    /// <summary>
    /// Default implementation of <see cref="ILiquidViewFileProviderAccessor"/>.
    /// </summary>
    public class LiquidViewFileProviderAccessor : ILiquidViewFileProviderAccessor
    {
        /// <summary>
        /// Initializes a new instance of <see cref="LiquidViewFileProviderAccessor"/>.
        /// </summary>
        /// <param name="optionsAccessor">Accessor to <see cref="LiquidViewOptions"/>.</param>
        public LiquidViewFileProviderAccessor(IOptions<LiquidViewOptions> optionsAccessor)
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
        /// Gets the <see cref="IFileProvider"/> used to look up Liquid files.
        /// </summary>
        public IFileProvider FileProvider { get; }
    }
}