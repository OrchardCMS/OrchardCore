using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Microsoft.Dnx.Runtime;
using Orchard.DependencyInjection;
using Orchard.Hosting.Extensions.Models;
using Orchard.Abstractions.Environment;
using Microsoft.Framework.Logging;

namespace Orchard.Hosting.Extensions.Loaders {
    public class DynamicExtensionLoader : IExtensionLoader {
        // TODO : Remove.
        public static readonly string[] ExtensionsVirtualPathPrefixes = { "~/Modules", "~/Themes" };

        private readonly IHostEnvironment _hostEnvironment;
        private readonly IAssemblyLoaderContainer _loaderContainer;
        private readonly IExtensionAssemblyLoader _extensionAssemblyLoader;
        private readonly ILogger _logger;

        public DynamicExtensionLoader(
            IHostEnvironment hostEnvironment,
            IAssemblyLoaderContainer container,
            IExtensionAssemblyLoader extensionAssemblyLoader,
            ILoggerFactory loggerFactory) {

            _hostEnvironment = hostEnvironment;
            _loaderContainer = container;
            _extensionAssemblyLoader = extensionAssemblyLoader;
            _logger = loggerFactory.CreateLogger<DynamicExtensionLoader>();
        }

        public string Name => GetType().Name;

        public int Order => 20;

        public void ExtensionActivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
        }

        public void ExtensionDeactivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
        }

        public bool IsCompatibleWithModuleReferences(ExtensionDescriptor extension, IEnumerable<ExtensionProbeEntry> references) {
            return true;
        }

        public ExtensionEntry Load(ExtensionDescriptor descriptor) {
            if (!ExtensionsVirtualPathPrefixes.Contains(descriptor.Location)) {
                return null;
            }

            var plocation = _hostEnvironment.MapPath(descriptor.Location);
            
            using (_loaderContainer.AddLoader(_extensionAssemblyLoader.WithPath(plocation))) {
                try {
                    var assembly = Assembly.Load(new AssemblyName(descriptor.Id));

                    _logger.LogInformation("Loaded referenced extension \"{0}\": assembly name=\"{1}\"", descriptor.Name, assembly.FullName);

                    return new ExtensionEntry {
                        Descriptor = descriptor,
                        Assembly = assembly,
                        ExportedTypes = assembly.ExportedTypes
                    };
                }
                catch (System.Exception ex) {
                    _logger.LogError(string.Format("Error trying to load extension {0}", descriptor.Id), ex);
                    throw;
                }

            }
        }

        public ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            return null;
        }

        public void ReferenceActivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry) {
        }

        public void ReferenceDeactivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry) {
        }
    }
}