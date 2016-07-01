using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions.Loaders
{
    public class AmbientExtensionLoader : IExtensionLoader
    {
        private readonly IExtensionLibraryService _extensionLibraryService;
        private readonly ILogger _logger;

        public AmbientExtensionLoader(
            IExtensionLibraryService extensionLibraryService,
            ILogger<AmbientExtensionLoader> logger)
        {
            _extensionLibraryService = extensionLibraryService;
            _logger = logger;
        }

        public string Name => GetType().Name;

        public int Order => 20;

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
            try
            {
                var assembly = _extensionLibraryService.LoadAmbientExtension(descriptor);

                if (assembly == null)
                {
                    return null;
                }

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Loaded referenced ambient extension \"{0}\": assembly name=\"{1}\"", descriptor.Name, assembly.FullName);
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