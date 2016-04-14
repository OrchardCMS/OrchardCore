using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Extensions.Folders;
using Microsoft.Extensions.Options;
using Orchard.FileSystem;

namespace Orchard.Environment.Extensions.Loaders
{
    public class DynamicExtensionLoader : IExtensionLoader
    {
        private readonly string[] ExtensionsSearchPaths;

        private readonly IHostEnvironment _hostEnvironment;
        private readonly IOrchardFileSystem _fileSystem;

        public DynamicExtensionLoader(
            IOptions<ExtensionHarvestingOptions> optionsAccessor,
            IHostEnvironment hostEnvironment,
            IOrchardFileSystem fileSystem)
        {
            ExtensionsSearchPaths = optionsAccessor.Value.ModuleLocationExpanders.SelectMany(x => x.SearchPaths).ToArray();
            _hostEnvironment = hostEnvironment;
            _fileSystem = fileSystem;
        }

        public string Name => GetType().Name;

        public int Order => 100;

        public void ExtensionActivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension)
        {
        }

        public void ExtensionDeactivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension)
        {
        }

        public bool IsCompatibleWithModuleReferences(ExtensionDescriptor extension, IEnumerable<ExtensionProbeEntry> references)
        {
            return true;
        }

        public ExtensionEntry Load(ExtensionDescriptor descriptor)
        {
            if (ExtensionsSearchPaths == null || descriptor == null || !ExtensionsSearchPaths.Contains(descriptor.Location))
            {
                return null;
            }

            var directory = _fileSystem.GetDirectoryInfo(descriptor.Location);

            return null;
        }

        public ExtensionProbeEntry Probe(ExtensionDescriptor descriptor)
        {
            return null;
        }

        public void ReferenceActivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry)
        {
        }

        public void ReferenceDeactivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry)
        {
        }
    }
}