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
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions.Compilers;
using Orchard.Environment.Extensions.FileSystem;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystem;
using Orchard.Localization;

namespace Orchard.Environment.Extensions
{
    public class ExtensionLibraryService : IExtensionLibraryService
    {
        private readonly ApplicationPartManager _applicationPartManager;
        private readonly IOrchardFileSystem _fileSystem;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary <string, bool> _loadedAssemblies = new ConcurrentDictionary<string, bool>();
        private object _applicationAssembliesNamesLock = new object();
        private bool _applicationAssembliesNamesInitialized;
        private List<string> _applicationAssembliesNames;
        private object _metadataReferencesLock = new object();
        private bool _metadataReferencesInitialized;
        private List<MetadataReference> _metadataReferences;

        public Localizer T { get; set; }

        public ExtensionLibraryService(
            ApplicationPartManager applicationPartManager,
            IOrchardFileSystem fileSystem,
            ILogger<ExtensionLibraryService> logger)
        {
            _applicationPartManager = applicationPartManager;
            _fileSystem = fileSystem;
            _logger = logger;
            T = NullLocalizer.Instance;
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
            return DependencyContext.Default.GetDefaultAssemblyNames().Select(x => x.Name).ToList();
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
            var extensionPath = _fileSystem.GetExtensionFileProvider(descriptor, _logger).RootPath;
            var projectContext = ProjectContext.CreateContextForEachFramework(extensionPath).FirstOrDefault();

            if (projectContext == null)
                return null;

            // Runtime loaded assemblies
            var loadedAssemblies = new HashSet<string>(_loadedAssemblies.Select(x => x.Key), StringComparer.OrdinalIgnoreCase);

            // Check if already runtime loaded
            if (!loadedAssemblies.Add(projectContext.RootProject.Identity.Name))
            {
                // Then load it as an ambient assembly
                return Assembly.Load(new AssemblyName(projectContext.RootProject.Identity.Name));
            }

            // Ambient assemblies
            var assemblyNames = new HashSet<string>(ApplicationAssemblyNames(), StringComparer.OrdinalIgnoreCase);

            // TODO: find a way to select the right configuration
            var libraryExporter = projectContext.CreateExporter("Debug");

            // Compile the extension if needed
            var compiler = new CSharpExtensionCompiler();
            var success = compiler.Compile(projectContext, "Debug");
            var diagnostics = compiler.Diagnostics;

            if (success)
            {
                if (_logger.IsEnabled(LogLevel.Information) && !diagnostics.Any())
                {
                     _logger.LogInformation("{0} was successfully compiled", descriptor.Id);
                }
                else if (_logger.IsEnabled(LogLevel.Warning))
                {
                    // TODO: log diagnostics messages
                     _logger.LogWarning("{0} was compiled but has warnings", descriptor.Id);
                }
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    // TODO: log diagnostics messages
                     _logger.LogError("{0} compilation failed", descriptor.Id);
                }
            }

            // Load and mark the assembly as loaded
            var assemblyPath = projectContext.GetOutputPaths("Debug").CompilationFiles.Assembly;
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            _loadedAssemblies[assembly.GetName().Name] = true;

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
                        if (!assemblyNames.Contains(itemName))
                        {
                            // Load from the extension lib folder
                            var itemFileName = Path.GetFileName(item.Path);
                            var path = Path.Combine(projectContext.ProjectDirectory, "lib", itemFileName);

                            // Check if file exists and not already loaded
                            if (File.Exists(path) && loadedAssemblies.Add(itemName))
                            {
                                // Load and mark the assembly as loaded
                                var itemAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                                _loadedAssemblies[itemAssembly.GetName().Name] = true;
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
                        if (!assemblyNames.Contains(asset.Name))
                        {
                            // Check if not already loaded
                            if (loadedAssemblies.Add(asset.Name))
                            {
                                // Load and mark the assembly as loaded
                                var assetAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(asset.ResolvedPath);
                                _loadedAssemblies[assetAssembly.GetName().Name] = true;
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
