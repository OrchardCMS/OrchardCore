using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Orchard.Environment.Extensions
{
    public class ShellExtensionProvider : IExtensionProvider
    {
        private readonly IFileProvider _fileProvider;
        private readonly IFeaturesProvider _featuresProvider;
        private readonly ShellOptions _shellOptions;

        /// <summary>
        /// Initializes a new instance of a ExtensionProvider at the given root directory.
        /// </summary>
        /// <param name="hostingEnvironment">hostingEnvironment containing the fileproviders.</param>
        /// <param name="featureManager">The feature manager.</param>
        public ShellExtensionProvider(
            IHostingEnvironment hostingEnvironment,
            IEnumerable<IFeaturesProvider> featureProviders,
            IOptions<ShellOptions> shellOptionsAccessor)
        {
            _fileProvider = hostingEnvironment.ContentRootFileProvider;
            _featuresProvider = new CompositeFeaturesProvider(featureProviders);
            _shellOptions = shellOptionsAccessor.Value;
        }

        public double Order { get { return 90D; } }

        /// <summary>
        /// Locate an extension at the given path by directly mapping path segments to physical directories.
        /// </summary>
        /// <param name="subpath">A path under the root directory</param>
        /// <returns>The extension information. null returned if extension does not exist</returns>
        public IExtensionInfo GetExtensionInfo(IManifestInfo manifestInfo, string subPath)
        {
            var shellRoot = Path.Combine(_shellOptions.ShellsRootContainerName, _shellOptions.ShellsContainerName);
            if (!subPath.StartsWith(shellRoot))
            {
                return null;
            }

            var path = Path.GetDirectoryName(subPath);
            var name = Path.GetFileName(subPath);

            var extension = _fileProvider
                .GetDirectoryContents(path)
                .First(content => content.Name == name);

            // sites1/modules/modules1
            var subShellPath = subPath.Remove(0, shellRoot.Length + 1);
            var shellName = subShellPath.Remove(subShellPath.IndexOf(Path.DirectorySeparatorChar));
            var id = string.Format("{0}_{1}", shellName, name);

            return new ShellExtensionInfo(id, shellName, extension, subPath, manifestInfo, (mi, ei) => {
                return _featuresProvider.GetFeatures(ei, mi);
            });
        }
    }
}
