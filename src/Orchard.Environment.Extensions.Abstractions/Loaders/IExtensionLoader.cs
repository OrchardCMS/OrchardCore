using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions.Loaders
{
    public class ExtensionProbeEntry
    {
        public ExtensionDescriptor Descriptor { get; set; }
        public IExtensionLoader Loader { get; set; }
        public int Priority { get; set; }
        public string VirtualPath { get; set; }
        public IEnumerable<string> VirtualPathDependencies { get; set; }
    }

    public class ExtensionReferenceProbeEntry
    {
        public ExtensionDescriptor Descriptor { get; set; }
        public IExtensionLoader Loader { get; set; }
        public string Name { get; set; }
        public string VirtualPath { get; set; }
    }

    public class ExtensionCompilationReference
    {
        public string AssemblyName { get; set; }
        public string BuildProviderTarget { get; set; }
    }

    public interface IExtensionLoader
    {
        int Order { get; }
        string Name { get; }

        void ReferenceActivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry);
        void ReferenceDeactivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry);
        bool IsCompatibleWithModuleReferences(ExtensionDescriptor extension, IEnumerable<ExtensionProbeEntry> references);

        ExtensionProbeEntry Probe(ExtensionDescriptor descriptor);
        ExtensionEntry Load(ExtensionDescriptor descriptor);

        void ExtensionActivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension);
        void ExtensionDeactivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension);
    }
}