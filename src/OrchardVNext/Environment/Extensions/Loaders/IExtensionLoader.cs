using System.Collections.Generic;
using System.Reflection;
using OrchardVNext.Environment.Extensions.Models;
using OrchardVNext.FileSystems.Dependencies;

namespace OrchardVNext.Environment.Extensions.Loaders {
    public class ExtensionProbeEntry {
        public ExtensionDescriptor Descriptor { get; set; }
        public IExtensionLoader Loader { get; set; }
        public int Priority { get; set; }
        public string VirtualPath { get; set; }
        public IEnumerable<string> VirtualPathDependencies { get; set; }
    }

    public class ExtensionReferenceProbeEntry {
        public ExtensionDescriptor Descriptor { get; set; }
        public IExtensionLoader Loader { get; set; }
        public string Name { get; set; }
        public string VirtualPath { get; set; }
    }

    public class ExtensionCompilationReference {
        public string AssemblyName { get; set; }
        public string BuildProviderTarget { get; set; }
    }

    public interface IExtensionLoader {
        int Order { get; }
        string Name { get; }

        IEnumerable<ExtensionReferenceProbeEntry> ProbeReferences(ExtensionDescriptor extensionDescriptor);
        Assembly LoadReference(DependencyReferenceDescriptor reference);
        void ReferenceActivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry);
        void ReferenceDeactivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry);
        bool IsCompatibleWithModuleReferences(ExtensionDescriptor extension, IEnumerable<ExtensionProbeEntry> references);

        ExtensionProbeEntry Probe(ExtensionDescriptor descriptor);
        ExtensionEntry Load(ExtensionDescriptor descriptor);

        void ExtensionActivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension);
        void ExtensionDeactivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension);
        void ExtensionRemoved(ExtensionLoadingContext ctx, DependencyDescriptor dependency);

        /// <summary>
        /// Return a list of references required to compile a component (e.g. a Razor or WebForm view)
        /// depending on the given module. 
        /// Each reference can either be an assembly name or a file to pass to the 
        /// IBuildManager.GetCompiledAssembly() method (e.g. a module .csproj project file).
        /// </summary>
        IEnumerable<ExtensionCompilationReference> GetCompilationReferences(DependencyDescriptor dependency);
        /// <summary>
        /// Return the list of dependencies (as virtual path) of the given module.
        /// If any of the dependency returned in the list is updated, a component depending 
        /// on the assembly produced for the module must be re-compiled.
        /// For example, Razor or WebForms views needs to be recompiled when a dependency of a module changes.
        /// </summary>
        IEnumerable<string> GetVirtualPathDependencies(DependencyDescriptor dependency);
    }
}