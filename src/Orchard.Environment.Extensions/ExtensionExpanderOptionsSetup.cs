using Microsoft.Extensions.Options;

namespace Orchard.Environment.Extensions
{
    /// <summary>
    /// Sets up default options for <see cref="ExtensionOptions"/>.
    /// </summary>
    public class ExtensionExpanderOptionsSetup : ConfigureOptions<ExtensionExpanderOptions>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ExtensionOptions"/>.
        /// </summary>
        public ExtensionExpanderOptionsSetup()
            : base(options => { })
        {
        }
    }
}