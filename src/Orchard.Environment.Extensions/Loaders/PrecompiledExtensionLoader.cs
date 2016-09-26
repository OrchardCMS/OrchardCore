using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions.Loaders
{
    public class PrecompiledExtensionLoader : IExtensionLoader
    {
        private readonly string[] ExtensionsSearchPaths;

        private readonly IExtensionLibraryService _extensionLibraryService;
        private readonly ILogger _logger;

        public PrecompiledExtensionLoader(
            IOptions<ExtensionHarvestingOptions> optionsAccessor,
            IExtensionLibraryService extensionLibraryService,
            ILogger<PrecompiledExtensionLoader> logger)
        {
            ExtensionsSearchPaths = optionsAccessor.Value.ExtensionLocationExpanders.SelectMany(x => x.SearchPaths).ToArray();
            _extensionLibraryService = extensionLibraryService;
            _logger = logger;
        }

        public string Name => GetType().Name;

        public int Order => 30;

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
            if (!ExtensionsSearchPaths.Contains(descriptor.Location))
            {
                return null;
            }

            try
            {
                var assembly = _extensionLibraryService.LoadPrecompiledExtension(descriptor);
            
                if (assembly == null)
                {
                    return null;
                }

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Loaded referenced precompiled extension \"{0}\": assembly name=\"{1}\"", descriptor.Name, assembly.FullName);
                }

                return new ExtensionEntry
                {
                    Descriptor = descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.ExportedTypes
                };
            }
            catch
            {
                return null;
            }
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