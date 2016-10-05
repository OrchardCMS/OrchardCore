using Microsoft.Extensions.Options;

namespace Orchard.Environment.Extensions
{
    /// <summary>
    /// Sets up default options for <see cref="ExtensionProbingOptions"/>.
    /// </summary>
    public class ExtensionProbingOptionsSetup : ConfigureOptions<ExtensionProbingOptions>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ExtensionProbingOptions"/>.
        /// </summary>
        public ExtensionProbingOptionsSetup()
            : base(options => { })
        {
        }
    }
}