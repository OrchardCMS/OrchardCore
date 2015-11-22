using Microsoft.Extensions.OptionsModel;

namespace Orchard.Environment.Extensions.Folders
{
    /// <summary>
    /// Sets up default options for <see cref="ExtensionHarvestingOptions"/>.
    /// </summary>
    public class ExtensionHarvestingOptionsSetup : ConfigureOptions<ExtensionHarvestingOptions>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ExtensionHarvestingOptions"/>.
        /// </summary>
        public ExtensionHarvestingOptionsSetup()
            : base(options => { })
        {
        }
    }
}