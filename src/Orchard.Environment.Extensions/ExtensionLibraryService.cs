using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary <string, Assembly> _loadedAssemblies = new ConcurrentDictionary<string, Assembly>();
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
            var extensionPath = Path.Combine(descriptor.Location, descriptor.Id);
            var projectContext = ProjectContext.CreateContextForEachFramework(extensionPath).FirstOrDefault();

            if (projectContext == null)
                return null;

            // Ambient assemblies
            var assemblyNames = new HashSet<string>(ApplicationAssemblyNames(), StringComparer.OrdinalIgnoreCase);

            // TODO: find a way to select the right configuration
            var libraryExporter = projectContext.CreateExporter("Debug");

            // Compile the extension if needed
            var compiler = new CSharpExtensionCompiler();
            var success = compiler.Compile(projectContext, "Debug");
            var diagnostics = compiler.Diagnostics;

            // TODO: what's the best to do if !success
            // TODO: logging diagnostics warnings / errors

            Assembly assembly;
            var assemblyPath = projectContext.GetOutputPaths("Debug").CompilationFiles.Assembly;
            var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);

            // Check if the extension is already loaded
            if (_loadedAssemblies.TryGetValue(assemblyName, out assembly))
            {
                return assembly;
            }
            else
            {
                // Load and mark the assembly as loaded
                assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                _loadedAssemblies[assemblyName] = assembly;
            }

            // Load the extension dependencies
            foreach (var dependency in libraryExporter.GetDependencies())
            {
                var package = dependency.Library as PackageDescription;

                // Check for an unresolved package (e.g in production)
                if (package != null && !dependency.RuntimeAssemblyGroups.Any())
                {
                    // Load the package from the extension lib folder
                    foreach (var item in package.RuntimeAssemblies)
                    {
                        var itemName = Path.GetFileNameWithoutExtension(item.Path);

                        // Check if not ambient
                        if (assemblyNames.Add(itemName))
                        {
                            Assembly itemAssembly;

                            // Check if already loaded
                            if (_loadedAssemblies.TryGetValue(itemName, out itemAssembly))
                                continue;

                            // Load from the extension lib folder
                            var itemFileName = Path.GetFileName(item.Path);
                            var path = Path.Combine(projectContext.ProjectDirectory, "lib", itemFileName);

                            if (File.Exists(path))
                            {
                                // Load and mark the assembly as loaded
                                itemAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                                _loadedAssemblies[itemName] = itemAssembly;
                            }
                        }
                    }
                }
                else
                {
                    // Load the package or the project library
                    foreach (var asset in dependency.RuntimeAssemblyGroups.GetDefaultAssets())
                    {
                        // Check if not ambient
                        if (assemblyNames.Add(asset.Name))
                        {
                            Assembly assetAssembly;

                            // Check if not already loaded
                            if (!_loadedAssemblies.TryGetValue(asset.Name, out assetAssembly))
                            {
                                // Load and mark the assembly as loaded
                                assetAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(asset.ResolvedPath);
                                _loadedAssemblies[asset.Name] = assetAssembly;
                            }

                            if (package != null)
                            {
                                // Populate the extension lib folder with the package asset
                                var assetProbingPath = Path.Combine(extensionPath, "lib", asset.FileName);

                                if (!File.Exists(assetProbingPath)
                                    || File.GetLastWriteTimeUtc(asset.ResolvedPath) > File.GetLastWriteTimeUtc(assetProbingPath))
                                {
                                    Directory.CreateDirectory(Path.Combine(extensionPath, "lib"));
                                    File.Copy(asset.ResolvedPath, assetProbingPath, true);
                                }
                            }
                        }
                    }
                }
            }

            return assembly;
        }
    }
}
