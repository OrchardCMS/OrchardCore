using Microsoft.Extensions.Options;

namespace Orchard.Environment.Extensions.Manifests
{
    /// <summary>
    /// Sets up default options for <see cref="ManifestOptions"/>.
    /// </summary>
    public class ManifestOptionsSetup : ConfigureOptions<ManifestOptions>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ManifestOptions"/>.
        /// </summary>
        public ManifestOptionsSetup()
            : base(options => { })
        {
        }
    }
}