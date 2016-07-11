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
using Microsoft.DotNet.InternalAbstractions;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.ProjectModel.Compilation;
using Microsoft.DotNet.ProjectModel.Graph;
using Microsoft.DotNet.Tools.Common;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using NuGet.Frameworks;
using Orchard.Environment.Extensions.Compilers;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystem;
using Orchard.FileSystem.AppData;
using Orchard.Localization;

namespace Orchard.Environment.Extensions
{
    public class ExtensionLibraryService : IExtensionLibraryService
    {
        public const string ReleaseConfiguration = "Release";
        public const string ProbingDirectoryName = "Dependencies";
        public static string Configuration => _configuration.Value;
        private static readonly Lazy<string> _configuration = new Lazy<string>(GetConfiguration);
        private static readonly Object _syncLock = new Object();

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
            var assemblyNames = new HashSet<string>(DependencyContext.Default.GetDefaultAssemblyNames()
                .Select(x => x.Name), StringComparer.OrdinalIgnoreCase);

            assemblyNames.UnionWith(DependencyContext.Default.RuntimeLibraries.Select(x => x.Name));

            return assemblyNames;
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

            var projectContext = GetProjectContext(descriptor);

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

            var projectContext = GetProjectContext(descriptor);

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

        internal ProjectContext GetProjectContext(ExtensionDescriptor descriptor)
        {
            var extensionPath = Path.Combine(_fileSystem.RootPath, descriptor.Location, descriptor.Id);
            return GetProjectContextFromPath(extensionPath);
        }

        internal ProjectContext GetProjectContextFromPath(string projectPath)
        {
            return ProjectContext.CreateContextForEachFramework(projectPath).FirstOrDefault();
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
                     _logger.LogInformation($"{0} was successfully compiled", context.ProjectName());
                }
                else if (_logger.IsEnabled(LogLevel.Warning))
                {
                     _logger.LogWarning($"{0} was compiled but has warnings", context.ProjectName());

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
                     _logger.LogError($"{0} compilation failed", context.ProjectName());

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

            var runtimeIds = GetRuntimeIdentifiers();

            foreach (var dependency in libraryExporter.GetAllExports())
            {
                var library = dependency.Library as ProjectDescription;
                var package = dependency.Library as PackageDescription;

                // Check for an unresolved library
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

                            var resourceFileName = library.Identity.Name + ".resources.dll";
                            var assemblyFolderName = PathUtility.GetDirectoryName(assemblyFolderPath);

                            var resourceAssemblies = Directory.GetFiles(assemblyFolderPath, resourceFileName, SearchOption.AllDirectories)
                                .Union(Directory.GetFiles(_probingFolderPath, resourceFileName, SearchOption.AllDirectories));

                            foreach (var asset in resourceAssemblies)
                            {
                                var locale = Directory.GetParent(asset).Name
                                    .Replace(assemblyFolderName, String.Empty)
                                    .Replace(ProbingDirectoryName, String.Empty);

                                PopulateBinaryFolder(assemblyFolderPath, asset, locale);
                                PopulateProbingFolder(asset, locale);
                            }
                        }
                    }
                }
                // Check for an unresolved package
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

                    foreach (var asset in package.RuntimeTargets)
                    {
                        var assetName = Path.GetFileNameWithoutExtension(asset.Path);

                        if (!IsAmbientAssembly(assetName))
                        {
                            var assetFileName = Path.GetFileName(asset.Path);

                            var relativeFolderPath = !String.IsNullOrEmpty(asset.Runtime)
                                ? Path.GetDirectoryName(asset.Path) : String.Empty;

                            var assetResolvedPath = ResolveAssemblyPath(assemblyFolderPath, assetFileName, relativeFolderPath);

                            if (!String.IsNullOrEmpty(assetResolvedPath))
                            {
                                if (runtimeIds.Contains(asset.Runtime))
                                {
                                    if (!IsAssemblyLoaded(assetName))
                                    {
                                        LoadFromAssemblyPath(assetResolvedPath);
                                    }
                                }

                                PopulateBinaryFolder(assemblyFolderPath, assetResolvedPath, relativeFolderPath);
                                PopulateProbingFolder(assetResolvedPath, relativeFolderPath);
                            }
                        }
                    }

                    var runtimeAssets = new HashSet<string>(package.RuntimeAssemblies.Select(x => x.Path));

                    foreach (var asset in package.CompileTimeAssemblies)
                    {
                        var assetFileName = Path.GetFileName(asset.Path);

                        if (!IsAmbientAssembly(assetFileName) && !runtimeAssets.Contains(asset.Path))
                        {
                            var assetResolvedPath = ResolveAssemblyPath(assemblyFolderPath, assetFileName, CSharpExtensionCompiler.RefsDirectoryName);

                            if (!String.IsNullOrEmpty(assetResolvedPath))
                            {
                                PopulateBinaryFolder(assemblyFolderPath, assetResolvedPath, CSharpExtensionCompiler.RefsDirectoryName);
                                PopulateProbingFolder(assetResolvedPath, CSharpExtensionCompiler.RefsDirectoryName);
                            }
                        }
                    }

                    if (!IsAmbientAssembly(package.Identity.Name))
                    {
                        foreach (var asset in package.ResourceAssemblies)
                        {
                            if (asset.Properties.ContainsKey("locale"))
                            {
                                var locale = asset.Properties["locale"];
                                var assetFileName = Path.GetFileName(asset.Path);
                                var assetResolvedPath = ResolveAssemblyPath(assemblyFolderPath, assetFileName, locale);

                                if (!String.IsNullOrEmpty(assetResolvedPath))
                                {
                                    PopulateBinaryFolder(assemblyFolderPath, assetResolvedPath, locale);
                                    PopulateProbingFolder(assetResolvedPath, locale);
                                }
                            }
                        }
                    }
                }
                // Check for a precompiled library
                else if (library != null && !dependency.RuntimeAssemblyGroups.Any())
                {
                    if (!IsAmbientAssembly(library.Identity.Name))
                    {
                        var projectContext = GetProjectContextFromPath(library.Project.ProjectDirectory);

                        if (projectContext != null)
                        {
                            var assetFileName = GetAssemblyFileName(library.Identity.Name);
                            var outputPath = projectContext.GetOutputPaths(Configuration).CompilationOutputPath;
                            var assetResolvedPath = Path.Combine(outputPath, assetFileName);

                            if (!File.Exists(assetResolvedPath))
                            {
                                assetResolvedPath = ResolveAssemblyPath(assemblyFolderPath, assetFileName);
                            }

                            if (!String.IsNullOrEmpty(assetResolvedPath))
                            {
                                if (!IsAssemblyLoaded(library.Identity.Name))
                                {
                                    LoadFromAssemblyPath(assetResolvedPath);
                                }

                                PopulateBinaryFolder(assemblyFolderPath, assetResolvedPath);
                                PopulateProbingFolder(assetResolvedPath);
                            }

                            var compilationOptions = projectContext.ResolveCompilationOptions(Configuration);
                            var resourceAssemblies = CompilerUtility.GetCultureResources(projectContext.ProjectFile, outputPath, compilationOptions);

                            foreach (var asset in resourceAssemblies)
                            {
                                assetFileName = Path.GetFileName(asset.OutputFile);
                                assetResolvedPath = asset.OutputFile;

                                if (!File.Exists(assetResolvedPath))
                                {
                                    assetResolvedPath = ResolveAssemblyPath(assemblyFolderPath, assetFileName, asset.Culture);
                                }

                                if (!String.IsNullOrEmpty(assetResolvedPath))
                                {
                                    PopulateBinaryFolder(assemblyFolderPath, assetResolvedPath, asset.Culture);
                                    PopulateProbingFolder(assetResolvedPath, asset.Culture);
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var assetGroup in dependency.RuntimeAssemblyGroups)
                    {
                        foreach (var asset in assetGroup.Assets)
                        {
                            if (!IsAmbientAssembly(asset.Name))
                            {
                                if (runtimeIds.Contains(assetGroup.Runtime))
                                {
                                    if (!IsAssemblyLoaded(asset.Name))
                                    {
                                        LoadFromAssemblyPath(asset.ResolvedPath);
                                    }
                                }

                                var relativeFolderPath = !String.IsNullOrEmpty(assetGroup.Runtime)
                                    ? Path.GetDirectoryName(asset.RelativePath) : String.Empty;

                                PopulateBinaryFolder(assemblyFolderPath, asset.ResolvedPath, relativeFolderPath);
                                PopulateProbingFolder(asset.ResolvedPath, relativeFolderPath);
                            }
                        }
                    }

                    var runtimeAssets = new HashSet<LibraryAsset>(dependency.RuntimeAssemblyGroups.GetDefaultAssets());

                    foreach (var asset in dependency.CompilationAssemblies)
                    {
                        if (!IsAmbientAssembly(asset.Name) && !runtimeAssets.Contains(asset))
                        {
                            PopulateBinaryFolder(assemblyFolderPath, asset.ResolvedPath, CSharpExtensionCompiler.RefsDirectoryName);
                            PopulateProbingFolder(asset.ResolvedPath, CSharpExtensionCompiler.RefsDirectoryName);
                        }
                    }

                    if (!IsAmbientAssembly(dependency.Library.Identity.Name))
                    {
                        foreach (var asset in dependency.ResourceAssemblies)
                        {
                            var assetResolvedPath = asset.Asset.ResolvedPath;

                            if (!String.IsNullOrEmpty(assetResolvedPath) && File.Exists(assetResolvedPath))
                            {
                                PopulateBinaryFolder(assemblyFolderPath, assetResolvedPath, asset.Locale);
                                PopulateProbingFolder(assetResolvedPath, asset.Locale);
                            }
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

        private static IEnumerable<string> GetRuntimeIdentifiers()
        {
            var runtimeIds = new List<string>();
            // Add runtime-agnostic id
            runtimeIds.Add(String.Empty);

            var candidateRids = new List<string>();
            var runtimeId = DependencyContext.Default?.Target.Runtime;

            if (string.IsNullOrEmpty(runtimeId))
            {
                candidateRids.AddRange(RuntimeEnvironmentRidExtensions.GetAllCandidateRuntimeIdentifiers());
            }
            else
            {
                candidateRids.Add(runtimeId);
            }

            runtimeIds.AddRange(candidateRids);

            var fallbacksRids = DependencyContext.Default?.RuntimeGraph ?? Enumerable.Empty<RuntimeFallbacks>();

            foreach (var rid in candidateRids)
            {
                runtimeIds.AddRange(fallbacksRids.Where(r => r.Runtime.Equals(rid)).SelectMany(x => x.Fallbacks));
            }

            return runtimeIds.Distinct();
        }

        private bool IsAmbientExtension (ExtensionDescriptor descriptor)
        {
             return IsAmbientAssembly(descriptor.Id);
        }

        private bool IsDynamicContext (ProjectContext context)
        {
            var compilationOptions = context.ResolveCompilationOptions(Configuration);
            return CompilerUtility.GetCompilationSources(context, compilationOptions).Any();
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
            bool loaded;
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

        private string ResolveAssemblyPath(string binaryFolderPath, string assemblyName, string relativeFolderPath = null)
        {
            binaryFolderPath = !String.IsNullOrEmpty(relativeFolderPath)
                ? Path.Combine(binaryFolderPath, relativeFolderPath)
                : binaryFolderPath;

            var probingFolderPath = !String.IsNullOrEmpty(relativeFolderPath)
                ? Path.Combine(_probingFolderPath, relativeFolderPath)
                : _probingFolderPath;

            var binaryPath = Path.Combine(binaryFolderPath, assemblyName);
            var probingPath = Path.Combine(probingFolderPath, assemblyName);

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

        private void PopulateBinaryFolder(string binaryFolderPath, string assetPath, string relativeFolderPath = null)
        {
            if (!PathUtility.IsChildOfDirectory(binaryFolderPath, assetPath))
            {
                binaryFolderPath = !String.IsNullOrEmpty(relativeFolderPath)
                    ? Path.Combine(binaryFolderPath, relativeFolderPath)
                    : binaryFolderPath;

                var binaryPath = Path.Combine(binaryFolderPath, Path.GetFileName(assetPath));

                if (!File.Exists(binaryPath) || File.GetLastWriteTimeUtc(assetPath) > File.GetLastWriteTimeUtc(binaryPath))
                {
                    lock (_syncLock)
                    {
                        Directory.CreateDirectory(binaryFolderPath);
                        File.Copy(assetPath, binaryPath, true);
                    }
                }
            }
        }

        private void PopulateProbingFolder(string assetPath, string locale = null)
        {
            PopulateBinaryFolder(_probingFolderPath, assetPath, locale);
        }
    }
}
