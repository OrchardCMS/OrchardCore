using System;
using System.IO;
using System.Runtime.Loader;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.CodeAnalysis;
using Microsoft.DotNet.ProjectModel;
using Orchard.Environment.Extensions.Compilers;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions
{
    public class ExtensionLibraryService : IExtensionLibraryService
    {
        private readonly ApplicationPartManager _applicationPartManager;
        private object _applicationAssembliesNamesLock = new object();
        private bool _applicationAssembliesNamesInitialized;
        private List<string> _applicationAssembliesNames;
        private object _metadataReferencesLock = new object();
        private bool _metadataReferencesInitialized;
        private List<MetadataReference> _metadataReferences;

        public ExtensionLibraryService(ApplicationPartManager applicationPartManager)
        {
            _applicationPartManager = applicationPartManager;
        }

        private IEnumerable<string> ApplicationAssemblyNames()
        {
            return LazyInitializer.EnsureInitialized(
                ref _applicationAssembliesNames,
                ref _applicationAssembliesNamesInitialized,
                ref _applicationAssembliesNamesLock,
                GetApplicationAssemblyNames);
        }

        public IEnumerable<MetadataReference> MetadataReferences()
        {
            return LazyInitializer.EnsureInitialized(
                ref _metadataReferences,
                ref _metadataReferencesInitialized,
                ref _metadataReferencesLock,
                GetMetadataReferences);
        }

        private List<string> GetApplicationAssemblyNames()
        {
            var assemblyNames = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            var projectContext = ProjectContext.CreateContextForEachFramework("").FirstOrDefault();

            // TODO: find a way to select the right configuration
            var libraryExporter = projectContext.CreateExporter("Debug");

            foreach (var libraryExport in libraryExporter.GetAllExports())
            {
                foreach (var asset in libraryExport.RuntimeAssemblyGroups.GetDefaultAssets())
                {
                    assemblyNames.Add(asset.Name);
                }
            }

            return assemblyNames.ToList();
        }

        private List<MetadataReference> GetMetadataReferences()
        {
            var assemblyNames = new HashSet<string>(ApplicationAssemblyNames(), StringComparer.OrdinalIgnoreCase);
            var metadataReferences = new List<MetadataReference>();

            foreach (var applicationPart in _applicationPartManager.ApplicationParts)
            {
                var assembly = applicationPart as AssemblyPart;
                if (assembly != null && assemblyNames.Add(assembly.Assembly.GetName().Name))
                {
                    var metadataReference = MetadataReference.CreateFromFile(assembly.Assembly.Location);
                    metadataReferences.Add(metadataReference);
                }
            }

            return metadataReferences;
        }

        public Assembly LoadExternalAssembly(ExtensionDescriptor descriptor)
        {
            var projectContext = ProjectContext.CreateContextForEachFramework(Path.Combine(descriptor.Location, descriptor.Id)).FirstOrDefault();

            if (projectContext == null)
                return null;

            var assemblyNames = new HashSet<string>(ApplicationAssemblyNames(), StringComparer.OrdinalIgnoreCase);

            // TODO: find a way to select the right configuration
            var libraryExporter = projectContext.CreateExporter("Debug");

            Assembly assembly = null;
            foreach (var libraryExport in libraryExporter.GetAllExports())
            {
                foreach (var asset in libraryExport.RuntimeAssemblyGroups.GetDefaultAssets())
                {
                    if (assemblyNames.Add(asset.Name)) {
                        try
                        {
                            if (asset.Name == projectContext.ProjectFile.Name) {
                                var compiler = new CSharpExtensionCompiler();
                                var success = compiler.Compile(projectContext, "Debug");
                                var diagnostics = compiler.Diagnostics;

                                // TODO: logging and see what's the best to do if !success
                                // if sucess && ! diagnostics.Any() => some Infos, compilation ok
                                // if success && diagnostics.Any()  => some Warnings in diagnostics
                                // if !success => diagnostics.Any() => some Errors in diagnostics
                                // Right now, if !success we try to use the last successful build
                            }

                            var loadedAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(asset.ResolvedPath);
                            if (loadedAssembly.GetName().Name == projectContext.ProjectFile.Name)
                                assembly = loadedAssembly;
                        }
                        catch
                        {
                        }
                    }
                }
            }

            return assembly;
        }
    }
}
