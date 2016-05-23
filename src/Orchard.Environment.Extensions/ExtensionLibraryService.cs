using System;
using System.IO;
using System.Runtime.Loader;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.DotNet.ProjectModel;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Extensions.Compilers;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions
{
    public class ExtensionLibraryService : IExtensionLibraryService
    {
        private readonly IServiceCollection _applicationServices;
        private object _applicationAssembliesNamesLock = new object();
        private bool _applicationAssembliesNamesInitialized;
        private List<string> _applicationAssembliesNames;
        private object _metadataReferencesLock = new object();
        private bool _metadataReferencesInitialized;
        private List<MetadataReference> _metadataReferences;

        public ExtensionLibraryService(IServiceCollection applicationServices)
        {
            _applicationServices = applicationServices;
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
            var assemblyPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var metadataReferences = new List<MetadataReference>();

            var extensionManager = _applicationServices.BuildServiceProvider().GetService<IExtensionManager>();

            foreach (var extension in extensionManager.AvailableExtensions())
            {
                var extensionEntry = extensionManager.LoadExtension(extension);

                if (assemblyPaths.Add(extensionEntry.Assembly.GetName().Name)) {
                    var metadataReference = MetadataReference.CreateFromFile(extensionEntry.Assembly.Location);
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
                            if (asset.Name == descriptor.Id) {
                                //if (!File.Exists(asset.ResolvedPath)) {
                                    var location = Path.Combine(descriptor.Location, descriptor.Id);


                // We can do this but it relies on the dotnet cli sdk 
                //Command.CreateDotNet("build", new [] { location }).Execute();


                // With an adapted compiler we only need to embed "csc.dll" and "csc.runtimeconfig.json"
                var success = new CSharpExtensionCompiler().Compile(projectContext, "Debug", projectContext.RootDirectory);


                                }
                            //}

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
