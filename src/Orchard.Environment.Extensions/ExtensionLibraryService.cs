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

        public Assembly LoadAmbientExtension(ExtensionDescriptor descriptor)
        {
            // Check if ambient
            if (ApplicationAssemblyNames().Contains(descriptor.Id))
            {
                return Assembly.Load(new AssemblyName(descriptor.Id));
            }

            return null;
        }

        public Assembly LoadPrecompiledExtension(ExtensionDescriptor descriptor)
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

            // Resolve the precompiled assembly output path
            var assemblyPath = ResolveAssemblyOutputPath(extensionPath, descriptor.Id);

            // Check if the precompiled assembly exists
            if (String.IsNullOrEmpty(assemblyPath))
            {
                return null;
            }

            // Load and mark the assembly as loaded
            var assembly = LoadFromAssemblyPath(assemblyPath);

            PopulateProbingFolder(assemblyPath);

            var dependencyContext = DependencyContext.Load(assembly);

            // Check for a valid context
            if (dependencyContext == null)
            {
                return null;
            }

            var assemblyFolderPath = Path.GetDirectoryName(assemblyPath);

            // Load the extension dependencies
            foreach (var assetPath in dependencyContext.RuntimeLibraries
                .SelectMany(library => library.RuntimeAssemblyGroups
                .SelectMany(a => a.AssetPaths)))
            {
                // Check if not ambient
                if (!ApplicationAssemblyNames().Contains(Path.GetFileNameWithoutExtension(assetPath)))
                {
                    var assetResolvedPath = ResolveAssetPath(assemblyFolderPath, Path.GetFileName(assetPath));

                    if (assetResolvedPath != null)
                    {
                        LoadFromAssemblyPath(assetResolvedPath);
                        PopulateProbingFolder(assetResolvedPath);
                    }
                }
            }

            return null;
        }

        public Assembly LoadDynamicExtension(ExtensionDescriptor descriptor)
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

            var config = GetConfig();
            var probingDirectoryPath = _appDataFolder.MapPath(ProbingDirectoryName);
            var compiler = new CSharpExtensionCompiler();

            // Compile the extension if needed
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

           // Set up output paths
            var outputPaths = projectContext.GetOutputPaths(config);
            var assemblyPath = outputPaths.CompilationFiles.Assembly;
            var assemblyFolderPath = outputPaths.CompilationOutputPath;

            // Load and mark the assembly as loaded
            var assembly = LoadFromAssemblyPath(assemblyPath);

            // Populate the probing folder
            PopulateProbingFolder(assemblyPath);

            // Create the library exporter
            var libraryExporter = projectContext.CreateExporter(config);

            // Load the extension dependencies
            foreach (var dependency in libraryExporter.GetDependencies())
            {
                var library = dependency.Library as ProjectDescription;
                var package = dependency.Library as PackageDescription;

                // Check for an unresolved library
                if (library != null && !library.Resolved)
                {
                    // Check if not ambient
                    if (!ApplicationAssemblyNames().Contains(library.Identity.Name))
                    {
                        var assetFileName = library.Identity.Name + FileNameSuffixes.DotNet.DynamicLib;
                        var assetResolvedPath = ResolveAssetPath(assemblyFolderPath, assetFileName);

                        if (assetResolvedPath != null)
                        {
                            LoadFromAssemblyPath(assetResolvedPath);
                            PopulateProbingFolder(assetResolvedPath);
                        }
                    }
                }
                // Check for an unresolved package
                else if (package != null && !package.Resolved)
                {
                    foreach (var asset in package.RuntimeAssemblies)
                    {
                        // Check if not ambient
                        if (!ApplicationAssemblyNames().Contains(Path.GetFileNameWithoutExtension(asset.Path)))
                        {
                            var assetFileName = Path.GetFileName(asset.Path);
                            var assetResolvedPath = ResolveAssetPath(assemblyFolderPath, assetFileName);

                            if (assetResolvedPath != null)
                            {
                                LoadFromAssemblyPath(assetResolvedPath);
                                PopulateProbingFolder(assetResolvedPath);
                            }
                        }
                    }
                }
                else
                {
                    // Load the package or the project library
                    foreach (var asset in dependency.RuntimeAssemblyGroups.GetDefaultAssets())
                    {
                        if (!ApplicationAssemblyNames().Contains(asset.Name))
                        {
                            LoadFromAssemblyPath(asset.ResolvedPath);
                            PopulateBinaryFolder(assemblyFolderPath, asset.ResolvedPath);
                            PopulateProbingFolder(asset.ResolvedPath);
                        }
                    }
                }
            }

            return assembly;
        }

        private string GetConfig()
        {
            var defines = DependencyContext.Default.CompilationOptions.Defines;
            return defines?.Contains(ReleaseConfiguration, StringComparer.OrdinalIgnoreCase) == true
                ? ReleaseConfiguration : Constants.DefaultConfiguration;
        }

        private string ResolveAssemblyOutputPath(string rootPath, string name)
        {
            return Directory.GetFiles(Path.Combine(rootPath, Constants.BinDirectoryName, GetConfig()),
                name + FileNameSuffixes.DotNet.DynamicLib, SearchOption.AllDirectories).FirstOrDefault();
        }

        private string ResolveAssetPath(string folderPath, string fileName)
        {
            var resolvedPath = Path.Combine(folderPath, fileName);

            if (!File.Exists(resolvedPath))
            {
                var probingFolderPath = _appDataFolder.MapPath(ProbingDirectoryName);
                resolvedPath = Path.Combine(probingFolderPath, fileName);

                if (!File.Exists(resolvedPath))
                {
                    return null;
                }
            }

            return resolvedPath;
        }

        private Assembly LoadFromAssemblyPath(string path)
        {
            var loaded = false;

            var fileName = Path.GetFileNameWithoutExtension(path);

            if (_loadedAssemblies.TryGetValue(fileName, out loaded))
            {
                return null;
            }

            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            _loadedAssemblies[fileName] = true;

            return assembly;
        }

        private void PopulateBinaryFolder(string folderPath, string path)
        {
            var binaryPath = Path.Combine(folderPath, Path.GetFileName(path));

            if (!File.Exists(binaryPath) || File.GetLastWriteTimeUtc(path) > File.GetLastWriteTimeUtc(binaryPath))
            {
                File.Copy(path, binaryPath, true);
            }
        }

        private void PopulateProbingFolder(string assetPath)
        {
            var probingFolderPath = _appDataFolder.MapPath(ProbingDirectoryName);

            if (!String.Equals(Path.GetDirectoryName(assetPath), probingFolderPath, StringComparison.OrdinalIgnoreCase))
            {
                var assetFileName = Path.GetFileName(assetPath);
                var probingPath = Path.Combine(probingFolderPath, assetFileName);

                if (!File.Exists(probingPath) || File.GetLastWriteTimeUtc(assetPath) > File.GetLastWriteTimeUtc(probingPath))
                {
                    Directory.CreateDirectory(probingFolderPath);
                    File.Copy(assetPath, probingPath, true);
                }
            }
        }
    }
}
