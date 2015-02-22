using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Versioning;
using System.Linq;
using Microsoft.Framework.Runtime;
using OrchardVNext.Environment.Extensions.Models;
using OrchardVNext.FileSystems.VirtualPath;

namespace OrchardVNext.Environment.Extensions.Loaders {
    public class DynamicExtensionLoader : IExtensionLoader {
        public static readonly string[] ExtensionsVirtualPathPrefixes = { "~/Modules", "~/Themes" };

        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAssemblyLoaderContainer _loaderContainer;

        public DynamicExtensionLoader(
            IVirtualPathProvider virtualPathProvider,
            IServiceProvider serviceProvider,
            IAssemblyLoaderContainer container) {

            _virtualPathProvider = virtualPathProvider;
            _serviceProvider = serviceProvider;
            _loaderContainer = container;

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

            var plocation = _virtualPathProvider.MapPath(descriptor.Location);

            using (_loaderContainer.AddLoader(new ExtensionAssemblyLoader(plocation, _serviceProvider))) {
                var assembly = Assembly.Load(new AssemblyName(descriptor.Id));

                Logger.Information("Loaded referenced extension \"{0}\": assembly name=\"{1}\"", descriptor.Name, assembly.FullName);

                return new ExtensionEntry {
                    Descriptor = descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.ExportedTypes
                };
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

    internal class LibraryKey : ILibraryKey {
        public string Name { get; set; }
        public FrameworkName TargetFramework { get; set; }
        public string Configuration { get; set; }
        public string Aspect { get; set; }
    }
}