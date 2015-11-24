using System.Reflection;
using System.Linq;
using Microsoft.Dnx.Compilation;
using Microsoft.Dnx.Compilation.Caching;
using Microsoft.Dnx.Runtime;
using Orchard.DependencyInjection;
using Orchard.Environment.Extensions.Loaders;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.PlatformAbstractions;
using System;

namespace Orchard.Environment.Extensions
{
    public class ExtensionAssemblyLoader : IExtensionAssemblyLoader
    {
        private readonly IApplicationEnvironment _applicationEnvironment;
        //private readonly ICache _cache;
        private readonly IAssemblyLoadContextAccessor _assemblyLoadContextAccessor;
        private readonly IRuntimeEnvironment _runtimeEnvironment;
        private readonly IOrchardLibraryManager _libraryManager;
        private string _path;

        public ExtensionAssemblyLoader(
            IApplicationEnvironment applicationEnvironment,
            //ICache cache,
            IAssemblyLoadContextAccessor assemblyLoadContextAccessor,
            IRuntimeEnvironment runtimeEnvironment,
            IOrchardLibraryManager libraryManager)
        {
            _applicationEnvironment = applicationEnvironment;
            //_cache = cache;
            _assemblyLoadContextAccessor = assemblyLoadContextAccessor;
            _runtimeEnvironment = runtimeEnvironment;
            _libraryManager = libraryManager;
        }

        public IExtensionAssemblyLoader WithPath(string path)
        {
            _path = path;
            return this;
        }

        public Assembly Load(AssemblyName assemblyName)
        {
            var reference = _libraryManager.GetMetadataReference(assemblyName.Name);

            if (reference != null && reference is MetadataFileReference)
            {
                var fileReference = (MetadataFileReference)reference;

                var assembly = _assemblyLoadContextAccessor
                    .Default
                    .LoadFile(fileReference.Path);

                return assembly;
            }

            var projectPath = Path.Combine(_path, assemblyName.Name);
            if (!Project.HasProjectFile(projectPath))
            {
                return null;
            }

            var moduleContext = new ModuleLoaderContext(
                projectPath,
                _applicationEnvironment.RuntimeFramework);

            foreach (var lib in moduleContext.LibraryManager.GetLibraries())
            {
                _libraryManager.AddLibrary(lib);
            }

            var engine = new CompilationEngine(new CompilationEngineContext(
                _applicationEnvironment,
                _runtimeEnvironment,
                _assemblyLoadContextAccessor.Default,
                new CompilationCache()));

            var exporter = engine.CreateProjectExporter(
                moduleContext.Project, moduleContext.TargetFramework, _applicationEnvironment.Configuration);

            var exports = exporter.GetAllExports(moduleContext.Project.Name);
            foreach (var metadataReference in exports.MetadataReferences)
            {
                _libraryManager.AddMetadataReference(metadataReference);
            }

            var loadedProjectAssembly = engine.LoadProject(
                moduleContext.Project,
                _applicationEnvironment.RuntimeFramework,
                null,
                _assemblyLoadContextAccessor.Default,
                assemblyName);

            IList<LibraryDependency> flattenedList = moduleContext
                .Project
                .Dependencies
                .SelectMany(x => Flatten(x))
                .Where(x => x.Library.Type == LibraryTypes.Package)
                .Distinct()
                .ToList();

            foreach (var dependency in flattenedList)
            {
                foreach (var assemblyToLoad in dependency.Library.Assemblies)
                {
                    Assembly.Load(new AssemblyName(assemblyToLoad));
                }
            }

            return loadedProjectAssembly;
        }

        public IntPtr LoadUnmanagedLibrary(string name)
        {
            return IntPtr.Zero;
        }

        public static IList<LibraryDependency> Flatten(LibraryDependency root)
        {
            var flattened = new List<LibraryDependency> { root };

            var children = root.Library.Dependencies;

            if (children != null)
            {
                foreach (var child in children)
                {
                    flattened.AddRange(Flatten(child));
                }
            }

            return flattened;
        }
    }
}