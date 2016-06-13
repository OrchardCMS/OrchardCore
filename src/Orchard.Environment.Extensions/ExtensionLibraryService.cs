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
using Microsoft.DotNet.Cli.Compiler.Common;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.ProjectModel.Compilation;
using Microsoft.DotNet.ProjectModel.Files;
using Microsoft.DotNet.ProjectModel.Graph;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using NuGet.Frameworks;
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
        private static readonly Lazy<string> _configuration = new Lazy<string>(GetConfiguration);

        private readonly ApplicationPartManager _applicationPartManager;
        private readonly IOrchardFileSystem _fileSystem;
        private readonly IAppDataFolder _appDataFolder;
        private readonly string _probingFolderPath;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary <string, bool> _loadedAssemblies = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private object _applicationAssembliesNamesLock = new object();
        private bool _applicationAssembliesNamesInitialized;
        private HashSet<string> _applicationAssembliesNames;
        private object _metadataReferencesLock = new object();
        private bool _metadataReferencesInitialized;
        private List<MetadataReference> _metadataReferences;

        public ExtensionLibraryService(
            ApplicationPartManager applicationPartManager,
            IOrchardFileSystem fileSystem,
            IAppDataFolder appDataFolder,
            ILogger<ExtensionLibraryService> logger)
        {
            _applicationPartManager = applicationPartManager;
            _fileSystem = fileSystem;
            _appDataFolder = appDataFolder;
            _probingFolderPath = _appDataFolder.MapPath(ProbingDirectoryName);
            _logger = logger;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        private static string Configuration => _configuration.Value;

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
            if (IsAmbientExtension(descriptor))
            {
                return Assembly.Load(new AssemblyName(descriptor.Id));
            }

            return null;
        }

        public Assembly LoadPrecompiledExtension(ExtensionDescriptor descriptor)
        {
            if (IsAmbientExtension(descriptor))
            {
                return null;
            }

            var projectContext = GetProject(descriptor);

            if (projectContext == null || IsDynamicContext(projectContext))
            {
                return null;
            }

            if (IsAssemblyLoaded(descriptor.Id))
            {
                return Assembly.Load(new AssemblyName(descriptor.Id));
            }

            return LoadProject(projectContext);
        }

        public Assembly LoadDynamicExtension(ExtensionDescriptor descriptor)
        {
            if (IsAmbientExtension(descriptor))
            {
                return null;
            }

            var projectContext = GetProject(descriptor);

            if (projectContext == null || !IsDynamicContext(projectContext))
            {
                return null;
            }

            if (IsAssemblyLoaded(descriptor.Id))
            {
                return Assembly.Load(new AssemblyName(descriptor.Id));
            }

            CompileProject(projectContext);

            return LoadProject(projectContext);
        }

        internal ProjectContext GetProject(ExtensionDescriptor descriptor)
        {
            var extensionPath = Path.Combine(_fileSystem.RootPath, descriptor.Location, descriptor.Id);
            return ProjectContext.CreateContextForEachFramework(extensionPath).FirstOrDefault();
        }

        internal void CompileProject(ProjectContext context)
        {

            var compiler = new CSharpExtensionCompiler();
            var success = compiler.Compile(context, Configuration, _probingFolderPath);
            var diagnostics = compiler.Diagnostics;

            if (success)
            {
                if (_logger.IsEnabled(LogLevel.Information) && !diagnostics.Any())
                {
                     _logger.LogInformation("{0} was successfully compiled", context.RootProject.Identity.Name);
                }
                else if (_logger.IsEnabled(LogLevel.Warning))
                {
                     _logger.LogWarning("{0} was compiled but has warnings", context.RootProject.Identity.Name);

                     foreach (var diagnostic in diagnostics)
                     {
                         _logger.LogWarning(diagnostic);
                     }
                }
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                     _logger.LogError("{0} compilation failed", context.RootProject.Identity.Name);

                     foreach (var diagnostic in diagnostics)
                     {
                         _logger.LogError(diagnostic);
                     }
                }
            }
        }

        internal Assembly LoadProject(ProjectContext context)
        {
            var outputPaths = context.GetOutputPaths(Configuration);
            var assemblyPath = outputPaths.CompilationFiles.Assembly;

            var assembly = LoadFromAssemblyPath(assemblyPath);
            PopulateProbingFolder(assemblyPath);

            var assemblyFolderPath = outputPaths.CompilationOutputPath;
            var libraryExporter = context.CreateExporter(Configuration);

            foreach (var dependency in libraryExporter.GetDependencies())
            {
                var library = dependency.Library as ProjectDescription;
                var package = dependency.Library as PackageDescription;

                if (library != null && !library.Resolved)
                {
                    if (!IsAmbientAssembly(library.Identity.Name))
                    {
                        var assetFileName = GetAssemblyFileName(library.Identity.Name);
                        var assetResolvedPath = ResolveAssemblyPath(assemblyFolderPath, assetFileName);

                        if (!String.IsNullOrEmpty(assetResolvedPath))
                        {
                            if (!IsAssemblyLoaded(library.Identity.Name))
                            {
                                LoadFromAssemblyPath(assetResolvedPath);
                            }

                            PopulateBinaryFolder(assemblyFolderPath, assetResolvedPath);
                            PopulateProbingFolder(assetResolvedPath);
                        }
                    }
                }
                else if (package != null && !package.Resolved)
                {
                    foreach (var asset in package.RuntimeAssemblies)
                    {
                        var assetName = Path.GetFileNameWithoutExtension(asset.Path);

                        if (!IsAmbientAssembly(assetName))
                        {
                            var assetFileName = Path.GetFileName(asset.Path);
                            var assetResolvedPath = ResolveAssemblyPath(assemblyFolderPath, assetFileName);

                            if (!String.IsNullOrEmpty(assetResolvedPath))
                            {
                                if (!IsAssemblyLoaded(assetName))
                                {
                                    LoadFromAssemblyPath(assetResolvedPath);
                                }

                                PopulateBinaryFolder(assemblyFolderPath, assetResolvedPath);
                                PopulateProbingFolder(assetResolvedPath);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var asset in dependency.RuntimeAssemblyGroups.GetDefaultAssets())
                    {
                        if (!IsAmbientAssembly(asset.Name))
                        {
                             if (!IsAssemblyLoaded(asset.Name))
                            {
                                LoadFromAssemblyPath(asset.ResolvedPath);
                            }

                            PopulateBinaryFolder(assemblyFolderPath, asset.ResolvedPath);
                            PopulateProbingFolder(asset.ResolvedPath);
                        }
                    }
                }
            }

            return assembly;
        }

        private static string GetConfiguration()
        {
            var defines = DependencyContext.Default.CompilationOptions.Defines;
            return defines?.Contains(ReleaseConfiguration, StringComparer.OrdinalIgnoreCase) == true
                ? ReleaseConfiguration : Constants.DefaultConfiguration;
        }

        private bool IsAmbientExtension (ExtensionDescriptor descriptor)
        {
             return IsAmbientAssembly(descriptor.Id);
        }

        private bool IsDynamicContext (ProjectContext context)
        {
            var sourceFiles = new List<string>();
            var compilationOptions = context.ResolveCompilationOptions(Configuration);

            if (compilationOptions.CompileInclude == null)
            {
                sourceFiles.AddRange(context.ProjectFile.Files.SourceFiles);
            }
            else
            {
                var includeFiles = IncludeFilesResolver.GetIncludeFiles(compilationOptions.CompileInclude, "/", diagnostics: null);
                sourceFiles.AddRange(includeFiles.Select(f => f.SourcePath));
            }

            return sourceFiles.Any();
        }

        private bool IsPrecompiledContext (ProjectContext context)
        {
            return !IsDynamicContext(context);
        }

        private bool IsAmbientAssembly(string assemblyName)
        {
             return ApplicationAssemblyNames().Contains(assemblyName);
        }

        private bool IsAssemblyLoaded(string assemblyName)
        {
            var loaded = false;
            return _loadedAssemblies.TryGetValue(assemblyName, out loaded);
        }

        private Assembly LoadFromAssemblyPath(string assemblyPath)
        {
            var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            _loadedAssemblies[assemblyName] = true;

            return assembly;
        }

        private string GetAssemblyFileName(string assemblyName)
        {
            return assemblyName + FileNameSuffixes.DotNet.DynamicLib;
        }

        private string GetAssemblyFolderPath(string rootPath, string framework)
        {
            return Path.Combine(rootPath, Constants.BinDirectoryName, Configuration,
                NuGetFramework.Parse(framework).GetShortFolderName());
        }

        private string ResolveAssemblyPath(string binaryFolderPath, string assemblyName)
        {
            var binaryPath = Path.Combine(binaryFolderPath, assemblyName);
            var probingPath = Path.Combine(_probingFolderPath, assemblyName);

            if (File.Exists(binaryPath))
            {
                if (File.Exists(probingPath))
                {
                    if (File.GetLastWriteTimeUtc(probingPath) > File.GetLastWriteTimeUtc(binaryPath))
                    {
                        return probingPath;
                    }
                }

                return binaryPath;
            }
            else if (File.Exists(probingPath))
            {
                return probingPath;
            }

            return null;
        }

        private void PopulateBinaryFolder(string binaryFolderPath, string assetPath)
        {
            if (!String.Equals(Path.GetDirectoryName(assetPath), binaryFolderPath, StringComparison.OrdinalIgnoreCase))
            {
                var binaryPath = Path.Combine(binaryFolderPath, Path.GetFileName(assetPath));

                if (!File.Exists(binaryPath) || File.GetLastWriteTimeUtc(assetPath) > File.GetLastWriteTimeUtc(binaryPath))
                {
                    Directory.CreateDirectory(binaryFolderPath);
                    File.Copy(assetPath, binaryPath, true);
                }
            }
        }

        private void PopulateProbingFolder(string assetPath)
        {
            PopulateBinaryFolder(_probingFolderPath, assetPath);
        }
    }
}
