using System;
using System.IO;
using OrchardCore.Environment.Shell.Configuration.Internal;

namespace Microsoft.Extensions.Configuration.Json
{
    /// <summary>
    /// A JSON file based <see cref="FileConfigurationProvider"/>.
    /// </summary>
    public class TenantJsonConfigurationProvider : FileConfigurationProvider
    {
        /// <summary>
        /// Initializes a new instance with the specified source.
        /// </summary>
        /// <param name="source">The source settings.</param>
        public TenantJsonConfigurationProvider(TenantJsonConfigurationSource source) : base(source) { }

        /// <summary>
        /// Loads the JSON data from a stream.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        public override void Load(Stream stream) => Data = JsonConfigurationParser.Parse(stream);

        /// <summary>
        /// Dispose the provider.
        /// </summary>
        /// <param name="disposing"><c>true</c> if invoked from <see cref="IDisposable.Dispose"/>.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(true);

            // OC: Will be part of 'FileConfigurationProvider' in a future version.
            // if (Source.OwnsFileProvider)
            {
                (Source.FileProvider as IDisposable)?.Dispose();
            }
        }
    }
}
