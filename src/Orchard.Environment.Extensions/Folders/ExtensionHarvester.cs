using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Orchard.Localization;
using Orchard.Environment.Extensions.Models;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions.Folders.ManifestParsers;
using Orchard.FileSystem;
using Orchard.Utility;
using ExtensionDescriptor = Orchard.Environment.Extensions.Models.ExtensionDescriptor;

namespace Orchard.Environment.Extensions.Folders {
    public class ExtensionHarvester : IExtensionHarvester {

        private readonly IClientFolder _clientFolder;
        private readonly IEnumerable<IManifestParser> _manifestParsers;
        private readonly ILogger _logger;

        public ExtensionHarvester(IClientFolder clientFolder,
            ILoggerFactory loggerFactory,
            IEnumerable<IManifestParser> manifestParsers) {
            _clientFolder = clientFolder;
            _manifestParsers = manifestParsers;
            _logger = loggerFactory.CreateLogger<ExtensionHarvester>();

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<ExtensionDescriptor> HarvestExtensions(IEnumerable<string> paths, string extensionType, string manifestName, bool manifestIsOptional) {
            return paths
                .SelectMany(path => HarvestExtensions(path, extensionType, manifestName, manifestIsOptional))
                .ToList();
        }

        private IEnumerable<ExtensionDescriptor> HarvestExtensions(string path, string extensionType, string manifestName, bool manifestIsOptional) {
            return AvailableExtensionsInFolder(path, extensionType, manifestName, manifestIsOptional).ToReadOnlyCollection();
        }

        private List<ExtensionDescriptor> AvailableExtensionsInFolder(string path, string extensionType, string manifestName, bool manifestIsOptional) {
            _logger.LogInformation("Start looking for extensions in '{0}'...", path);
            var subfolderPaths = _clientFolder.ListDirectories(path);
            var localList = new List<ExtensionDescriptor>();
            foreach (var subfolderPath in subfolderPaths) {
                var extensionId = Path.GetFileName(subfolderPath);
                var manifestPath = Path.Combine(subfolderPath, manifestName);
                try {
                    var descriptor = GetExtensionDescriptor(path, extensionId, extensionType, manifestPath, manifestIsOptional);

                    if (descriptor == null)
                        continue;

                    if (descriptor.Path != null && !descriptor.Path.IsValidUrlSegment()) {
                        _logger.LogError("The module '{0}' could not be loaded because it has an invalid Path ({1}). It was ignored. The Path if specified must be a valid URL segment. The best bet is to stick with letters and numbers with no spaces.",
                                     extensionId,
                                     descriptor.Path);
                        continue;
                    }

                    if (descriptor.Path == null) {
                        descriptor.Path = descriptor.Name.IsValidUrlSegment()
                                              ? descriptor.Name
                                              : descriptor.Id;
                    }

                    localList.Add(descriptor);
                }
                catch (Exception ex) {
                    // Ignore invalid module manifests
                    _logger.LogError(string.Format("The module '{0}' could not be loaded. It was ignored.", extensionId), ex);
                }
            }
            _logger.LogInformation("Done looking for extensions in '{0}': {1}", path, string.Join(", ", localList.Select(d => d.Id)));
            return localList;
        }
        
        private ExtensionDescriptor GetExtensionDescriptor(string locationPath, string extensionId, string extensionType, string manifestPath, bool manifestIsOptional) {
            var manifestText = _clientFolder.ReadFile(manifestPath);
            if (manifestText == null) {
                if (manifestIsOptional) {
                    manifestText = $"Id: {extensionId}";
                }
                else {
                    return null;
                }
            }

            var extension = new FileInfo(manifestPath).Extension;
            var parser = _manifestParsers.FirstOrDefault(x => x.Extension.Equals(extension, StringComparison.CurrentCultureIgnoreCase));
            return parser?.GetDescriptorForExtension(locationPath, extensionId, extensionType, manifestText);
        }
    }
}