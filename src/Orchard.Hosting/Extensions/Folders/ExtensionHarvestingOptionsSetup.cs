using Microsoft.Framework.OptionsModel;

namespace Orchard.Hosting.Extensions.Folders {
    /// <summary>
    /// Sets up default options for <see cref="ExtensionHarvestingOptions"/>.
    /// </summary>
    public class ExtensionHarvestingOptionsSetup : ConfigureOptions<ExtensionHarvestingOptions> {
        /// <summary>
        /// Initializes a new instance of <see cref="ExtensionHarvestingOptions"/>.
        /// </summary>
        public ExtensionHarvestingOptionsSetup()
            : base(options => {  }) {
            Order = -1000; 
        }
    }
}
