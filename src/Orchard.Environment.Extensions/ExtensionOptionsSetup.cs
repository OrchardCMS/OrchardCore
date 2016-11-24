using Microsoft.Extensions.Options;

namespace Orchard.Environment.Extensions
{
    /// <summary>
    /// Sets up default options for <see cref="ExtensionOptions"/>.
    /// </summary>
    public class ExtensionOptionsSetup : ConfigureOptions<ExtensionOptions>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ExtensionOptions"/>.
        /// </summary>
        public ExtensionOptionsSetup()
            : base(options => { })
        {
        }
    }
}