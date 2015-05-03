using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Framework.Runtime;
using OrchardVNext.Environment.Extensions.Models;
using OrchardVNext.FileSystems.VirtualPath;

namespace OrchardVNext.Environment.Extensions.Loaders {
    public class CoreExtensionLoader : IExtensionLoader {
        private const string CoreAssemblyName = "OrchardVNext.Core";
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAssemblyLoaderContainer _loaderContainer;

        public CoreExtensionLoader(
            IVirtualPathProvider virtualPathProvider,
            IServiceProvider serviceProvider,
            IAssemblyLoaderContainer container) {

            _virtualPathProvider = virtualPathProvider;
            _serviceProvider = serviceProvider;
            _loaderContainer = container;

        }

        public string Name => GetType().Name;

        public int Order => 10;

        public void ExtensionActivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
        }

        public void ExtensionDeactivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
        }

        public bool IsCompatibleWithModuleReferences(ExtensionDescriptor extension, IEnumerable<ExtensionProbeEntry> references) {
            return true;
        }

        public ExtensionEntry Load(ExtensionDescriptor descriptor) {

            if (!descriptor.Location.StartsWith("~/Core/")) {
                return null;
            }

            var plocation = _virtualPathProvider.MapPath("~/Core"); 

            using (_loaderContainer.AddLoader(new ExtensionAssemblyLoader(plocation, _serviceProvider))) {
                var assembly = Assembly.Load(new AssemblyName(CoreAssemblyName));

                Logger.Information("Loaded referenced extension \"{0}\": assembly name=\"{1}\"", descriptor.Name, assembly.FullName);

                return new ExtensionEntry {
                    Descriptor = descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.ExportedTypes.Where(x => IsTypeFromModule(x, descriptor))
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
        private static bool IsTypeFromModule(Type type, ExtensionDescriptor descriptor) {
            return (type.Namespace + ".").StartsWith(CoreAssemblyName + "." + descriptor.Id + ".");
        }
    }
}