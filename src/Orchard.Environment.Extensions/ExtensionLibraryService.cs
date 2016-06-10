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
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.ProjectModel.Graph;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions.Compilers;
using Orchard.Environment.Extensions.FileSystem;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystem;
using Orchard.FileSystem.AppData;
using Orchard.Localization;

namespace Orchard.Environment.Extensions
{
    public class ExtensionLibraryService : IExtensionLibraryService
    {
        private const string ProbingDirectoryName = "Dependencies";
        public static readonly string ReleaseConfiguration = "Release";

        private readonly ApplicationPartManager _applicationPartManager;
        private readonly IOrchardFileSystem _fileSystem;
        private readonly IAppDataFolder _appDataFolder;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary <string, bool> _loadedAssemblies = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private object _applicationAssembliesNamesLock = new object();
        private bool _applicationAssembliesNamesInitialized;
        private HashSet<string> _applicationAssembliesNames;
        private object _metadataReferencesLock = new object();
        private bool _metadataReferencesInitialized;
        private List<MetadataReference> _metadataReferences;

        public Localizer T { get; set; }

        public ExtensionLibraryService(
            ApplicationPartManager applicationPartManager,
            IOrchardFileSystem fileSystem,
            IAppDataFolder appDataFolder,
            ILogger<ExtensionLibraryService> logger)
        {
            _applicationPartManager = applicationPartManager;
            _fileSystem = fileSystem;
            _appDataFolder = appDataFolder;
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

        private HashSet<string> GetApplicationAssemblyNames()
        {
            return new HashSet<string>(DependencyContext.Default.GetDefaultAssemblyNames()
                .Select(x => x.Name), StringComparer.OrdinalIgnoreCase);
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

        public Assembly LoadAmbientAssembly(ExtensionDescriptor descriptor)
        {
            // Check if ambient
            if (ApplicationAssemblyNames().Contains(descriptor.Id))
            {
                return Assembly.Load(new AssemblyName(descriptor.Id));
            }

            return null;
        }

        public Assembly LoadPrecompiledAssembly(ExtensionDescriptor descriptor)
        {
            // Check if ambient
            if (ApplicationAssemblyNames().Contains(descriptor.Id))
            {
                return null;
            }

            var extensionPath = _fileSystem.GetExtensionFileProvider(descriptor, _logger).RootPath;

            // Check if all project files are there
            if (File.Exists(Path.Combine(extensionPath, Project.FileName)) && File.Exists(Path.Combine(extensionPath, LockFile.FileName)))
            {
                return null;
            }

            bool loaded;

            // Check if already runtime loaded
            if (_loadedAssemblies.TryGetValue(descriptor.Id, out loaded))
            {
                // Then load it as an ambient assembly
                return Assembly.Load(new AssemblyName(descriptor.Id));
            }

            // Select the compilation configuration
            var defines = DependencyContext.Default.CompilationOptions.Defines;
            var config = defines?.Contains(ReleaseConfiguration, StringComparer.OrdinalIgnoreCase) == true
                ? ReleaseConfiguration : Constants.DefaultConfiguration;

            // Resolve binaries output paths
            var probingDirectoryPath = _appDataFolder.MapPath(ProbingDirectoryName);
            var probingPath = Path.Combine(probingDirectoryPath, descriptor.Id + FileNameSuffixes.DotNet.DynamicLib);
            var assemblyPath = Directory.GetFiles(Path.Combine(extensionPath, Constants.BinDirectoryName, config),
                descriptor.Id + FileNameSuffixes.DotNet.DynamicLib, SearchOption.AllDirectories).FirstOrDefault();

            if (String.IsNullOrEmpty(assemblyPath) || !File.Exists(assemblyPath))
            {
                assemblyPath = probingPath;
                probingPath = null;

                if (String.IsNullOrEmpty(assemblyPath) || !File.Exists(assemblyPath))
                {
                    return null;
                }
            }

            // Load and mark the assembly as loaded
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            //_loadedAssemblies[descriptor.Id] = true;

            // Populate the probing folder with the extension assembly
            if (probingPath != null && (!File.Exists(probingPath) || File.GetLastWriteTimeUtc(assemblyPath) > File.GetLastWriteTimeUtc(probingPath)))
            {
                Directory.CreateDirectory(probingDirectoryPath);
                File.Copy(assemblyPath, probingPath, true);
            }


            var dependencyContext = DependencyContext.Load(assembly);

            // Check for a valid context
            if (dependencyContext == null)
            {
                return null;
            }

var test = dependencyContext.RuntimeLibraries.Where(library => library.Type == "project").ToList();
;

            return null;
        }

        public Assembly LoadDynamicAssembly(ExtensionDescriptor descriptor)
        {
            // Check if ambient
            if (ApplicationAssemblyNames().Contains(descriptor.Id))
            {
                return null;
            }

            var extensionPath = _fileSystem.GetExtensionFileProvider(descriptor, _logger).RootPath;

            // Check if not all project files are there
            if (!File.Exists(Path.Combine(extensionPath, Project.FileName)) || !File.Exists(Path.Combine(extensionPath, LockFile.FileName)))
            {
                return null;
            }

            var projectContext = ProjectContext.CreateContextForEachFramework(extensionPath).FirstOrDefault();

            // Check for a valid context
            if (projectContext == null)
            {
                return null;
            }

            bool loaded;

            // Check if already runtime loaded
            if (_loadedAssemblies.TryGetValue(projectContext.RootProject.Identity.Name, out loaded))
            {
                // Then load it as an ambient assembly
                return Assembly.Load(new AssemblyName(projectContext.RootProject.Identity.Name));
            }

            // Select the compilation configuration
            var defines = DependencyContext.Default.CompilationOptions.Defines;
            var config = defines?.Contains(ReleaseConfiguration, StringComparer.OrdinalIgnoreCase) == true
                ? ReleaseConfiguration : Constants.DefaultConfiguration;

            // Create the library exporter
            var libraryExporter = projectContext.CreateExporter(config);

            // Compile the extension if needed
            var compiler = new CSharpExtensionCompiler();
            var probingDirectoryPath = _appDataFolder.MapPath(ProbingDirectoryName);
            var success = compiler.Compile(projectContext, config, probingDirectoryPath);
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
            var assemblyPath = projectContext.GetOutputPaths(config).CompilationFiles.Assembly;
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            _loadedAssemblies[projectContext.RootProject.Identity.Name] = true;

            // Populate the probing folder with the extension assembly
            var probingPath = Path.Combine(probingDirectoryPath, projectContext.RootProject.Identity.Name + FileNameSuffixes.DotNet.DynamicLib);

            if (!File.Exists(probingPath) || File.GetLastWriteTimeUtc(assemblyPath) > File.GetLastWriteTimeUtc(probingPath))
            {
                Directory.CreateDirectory(probingDirectoryPath);
                File.Copy(assemblyPath, probingPath, true);
            }

            // Load the extension dependencies
            foreach (var dependency in libraryExporter.GetDependencies())
            {
                var library = dependency.Library as ProjectDescription;
                var package = dependency.Library as PackageDescription;

                // Check for an unresolved library
                if (library != null && !library.Resolved)
                {
                    var assemblyName = library.Identity.Name;

                    // Check if not ambient
                    if (!ApplicationAssemblyNames().Contains(assemblyName))
                    {
                        // Load from the probing folder
                        var fileName = assemblyName + FileNameSuffixes.DotNet.DynamicLib;
                        var path = Path.Combine(probingDirectoryPath, fileName);

                        // Check if file exists and not already loaded
                        if (File.Exists(path) && !_loadedAssemblies.TryGetValue(assemblyName, out loaded))
                        {
                            // Load and mark the assembly as loaded
                            var itemAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                            _loadedAssemblies[assemblyName] = true;
                        }
                    }
                }
                // Check for an unresolved package
                else if (package != null && !package.Resolved)
                {
                    // Load the package from the probing folder
                    foreach (var item in package.RuntimeAssemblies)
                    {
                        var assemblyName = Path.GetFileNameWithoutExtension(item.Path);

                        // Check if not ambient
                        if (!ApplicationAssemblyNames().Contains(assemblyName))
                        {
                            // Load from the probing folder
                            var fileName = Path.GetFileName(item.Path);
                            var path = Path.Combine(probingDirectoryPath, fileName);

                            // Check if file exists and not already loaded
                            if (File.Exists(path) && !_loadedAssemblies.TryGetValue(assemblyName, out loaded))
                            {
                                // Load and mark the assembly as loaded
                                AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                                _loadedAssemblies[assemblyName] = true;
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
                        if (!ApplicationAssemblyNames().Contains(asset.Name))
                        {
                            // Check if not already loaded
                            if (!_loadedAssemblies.TryGetValue(asset.Name, out loaded))
                            {
                                // Load and mark the assembly as loaded
                                AssemblyLoadContext.Default.LoadFromAssemblyPath(asset.ResolvedPath);
                                _loadedAssemblies[asset.Name] = true;
                            }

                            // Populate the probing folder with the library asset
                            var path = Path.Combine(probingDirectoryPath, asset.FileName);

                            if (!File.Exists(path) || File.GetLastWriteTimeUtc(asset.ResolvedPath) > File.GetLastWriteTimeUtc(path))
                            {
                                Directory.CreateDirectory(probingDirectoryPath);
                                File.Copy(asset.ResolvedPath, path, true);
                            }
                        }
                    }
                }
            }

            return assembly;
        }
    }
}
