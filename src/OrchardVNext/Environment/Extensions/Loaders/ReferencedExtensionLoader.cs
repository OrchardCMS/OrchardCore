//using OrchardVNext.Environment.Extensions.Models;
//using OrchardVNext.FileSystems.Dependencies;
//using OrchardVNext.FileSystems.VirtualPath;
//using Microsoft.Framework.Runtime;
//using System;

//namespace OrchardVNext.Environment.Extensions.Loaders {
//    /// <summary>
//    /// Load an extension by looking through the BuildManager referenced assemblies
//    /// </summary>
//    public class ReferencedExtensionLoader : ExtensionLoaderBase {
//        private readonly IVirtualPathProvider _virtualPathProvider;
//        private readonly IServiceProvider _serviceProvider;
//        private readonly ILibraryManager _libraryManager;
//        private readonly IAssemblyLoadContextFactory _assemblyLoadContextFactory;

//        public ReferencedExtensionLoader(IDependenciesFolder dependenciesFolder,
//            IVirtualPathProvider virtualPathProvider,
//            IServiceProvider serviceProvider,
//            ILibraryManager libraryManager,
//            IAssemblyLoadContextFactory assemblyLoadContextFactory)
//            : base(dependenciesFolder) {

//            _virtualPathProvider = virtualPathProvider;
//            _serviceProvider = serviceProvider;
//            _libraryManager = libraryManager;
//            _assemblyLoadContextFactory = assemblyLoadContextFactory;

//        }

//        public bool Disabled { get; set; }

//        public override int Order { get { return 20; } }

//        public override ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
//            if (Disabled)
//                return null;

//            var plocation = _virtualPathProvider.MapPath(_virtualPathProvider.Combine(descriptor.Location, descriptor.Id));
//            Project project = null;
//            if (!Project.TryGetProject(plocation, out project)) {
//                return null;
//            }

//            return new ExtensionProbeEntry {
//                Descriptor = descriptor,
//                Loader = this,
//                Priority = 100, // Higher priority because assemblies in ~/bin always take precedence
//                VirtualPath = project.ProjectDirectory,
//                VirtualPathDependencies = new[] { project.ProjectDirectory },
//            };
//        }

//        protected override ExtensionEntry LoadWorker(ExtensionDescriptor descriptor) {
//            if (Disabled)
//                return null;

//            var assembly = _assemblyLoadContextFactory.Create().Load(descriptor.Id);

//            Logger.Information("Loaded referenced extension \"{0}\": assembly name=\"{1}\"", descriptor.Name, assembly.FullName);

//            return new ExtensionEntry {
//                Descriptor = descriptor,
//                Assembly = assembly,
//                ExportedTypes = assembly.ExportedTypes
//            };
//        }
//    }
//}
