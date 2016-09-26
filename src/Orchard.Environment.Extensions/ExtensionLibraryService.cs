using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.CodeAnalysis;
using Microsoft.DotNet.Cli.Compiler.Common;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.InternalAbstractions;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.ProjectModel.Compilation;
using Microsoft.DotNet.Tools.Common;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuGet.Frameworks;
using Orchard.Environment.Extensions.Compilers;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Shell;
using Orchard.FileSystem;
using Orchard.Localization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Orchard.Environment.Extensions
{
    public class ExtensionLibraryService : IExtensionLibraryService
    {
        public const string ProbingDirectoryName = "Dependencies";
        public static string Configuration => _configuration.Value;
        private static readonly Lazy<string> _configuration = new Lazy<string>(GetConfiguration);
        private static readonly Object _syncLock = new Object();

        private static HashSet<string> ApplicationAssemblyNames => _applicationAssemblyNames.Value;
        private static readonly Lazy<HashSet<string>> _applicationAssemblyNames = new Lazy<HashSet<string>>(GetApplicationAssemblyNames);
        private static readonly ConcurrentDictionary<string, Lazy<Assembly>> _loadedAssemblies = new ConcurrentDictionary<string, Lazy<Assembly>>(StringComparer.OrdinalIgnoreCase);

        private readonly Lazy<List<MetadataReference>> _metadataReferences;
        private readonly ApplicationPartManager _applicationPartManager;
        private readonly IOrchardFileSystem _fileSystem;
        private readonly string _probingFolderPath;
        private readonly ILogger _logger;

        public ExtensionLibraryService(
            ApplicationPartManager applicationPartManager,
            IOrchardFileSystem fileSystem,
            IOptions<ShellOptions> shellOptionsAccessor,
            ILogger<ExtensionLibraryService> logger)
        {
            _metadataReferences = new Lazy<List<MetadataReference>>(GetMetadataReferences);
            _applicationPartManager = applicationPartManager;
            _fileSystem = fileSystem;
            _probingFolderPath = Path.Combine(shellOptionsAccessor.Value.ShellHostContainer.PhysicalPath, ProbingDirectoryName);
            _logger = logger;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<MetadataReference> MetadataReferences()
        {
            return _metadataReferences.Value;
        }

        private static HashSet<string> GetApplicationAssemblyNames()
        {
            return new HashSet<string>(DependencyContext.Default.RuntimeLibraries
                .SelectMany(library => library.RuntimeAssemblyGroups)
                .SelectMany(assetGroup => assetGroup.AssetPaths)
                .Select(path => Path.GetFileNameWithoutExtension(path)),
                StringComparer.OrdinalIgnoreCase);
        }

        private List<MetadataReference> GetMetadataReferences()
        {
            var assemblyNames = new HashSet<string>(ApplicationAssemblyNames, StringComparer.OrdinalIgnoreCase);
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
                return _loadedAssemblies[descriptor.Id].Value;
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
                return _loadedAssemblies[descriptor.Id].Value;
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

            if (success && diagnostics.Count == 0)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("{0} was successfully compiled", context.ProjectName());
                }
            }
            else if (success && diagnostics.Count > 0)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning("{0} was compiled but has warnings", context.ProjectName());

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
                            LoadFromAssemblyPath(assetResolvedPath);
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
                                PopulateRuntimeFolder(asset, locale);
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
                                LoadFromAssemblyPath(assetResolvedPath);
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
                                    LoadFromAssemblyPath(assetResolvedPath);
                                }

                                PopulateBinaryFolder(assemblyFolderPath, assetResolvedPath, relativeFolderPath);
                                PopulateProbingFolder(assetResolvedPath, relativeFolderPath);
                            }
                        }
                    }

                    var runtimeAssets = new HashSet<string>(package.RuntimeAssemblies.Select(x => x.Path), StringComparer.OrdinalIgnoreCase);

                    foreach (var asset in package.CompileTimeAssemblies)
                    {
                        var assetFileName = Path.GetFileName(asset.Path);

                        if (!IsAmbientAssembly(assetFileName) && !runtimeAssets.Contains(asset.Path))
                        {
                            var assetResolvedPath = ResolveAssemblyPath(assemblyFolderPath, assetFileName, CompilerUtility.RefsDirectoryName);

                            if (!String.IsNullOrEmpty(assetResolvedPath))
                            {
                                PopulateBinaryFolder(assemblyFolderPath, assetResolvedPath, CompilerUtility.RefsDirectoryName);
                                PopulateProbingFolder(assetResolvedPath, CompilerUtility.RefsDirectoryName);
                            }
                        }
                    }

                    if (!IsAmbientAssembly(package.Identity.Name))
                    {
                        foreach (var asset in package.ResourceAssemblies)
                        {
                            string locale;
                            if (asset.Properties.TryGetValue(CompilerUtility.LocaleLockFilePropertyName, out locale))
                            {
                                var assetFileName = Path.GetFileName(asset.Path);
                                var assetResolvedPath = ResolveAssemblyPath(assemblyFolderPath, assetFileName, locale);

                                if (!String.IsNullOrEmpty(assetResolvedPath))
                                {
                                    PopulateBinaryFolder(assemblyFolderPath, assetResolvedPath, locale);
                                    PopulateProbingFolder(assetResolvedPath, locale);
                                    PopulateRuntimeFolder(assetResolvedPath, locale);
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
                                LoadFromAssemblyPath(assetResolvedPath);
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
                                    PopulateRuntimeFolder(assetResolvedPath, asset.Culture);
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
                                    LoadFromAssemblyPath(asset.ResolvedPath);
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
                            PopulateBinaryFolder(assemblyFolderPath, asset.ResolvedPath, CompilerUtility.RefsDirectoryName);
                            PopulateProbingFolder(asset.ResolvedPath, CompilerUtility.RefsDirectoryName);
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
                                PopulateRuntimeFolder(assetResolvedPath, asset.Locale);
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
            return defines?.Contains(CompilerUtility.ReleaseConfiguration, StringComparer.OrdinalIgnoreCase) == true
                ? CompilerUtility.ReleaseConfiguration : CompilerUtility.DefaultConfiguration;
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
            return ApplicationAssemblyNames.Contains(assemblyName);
        }

        private bool IsAssemblyLoaded(string assemblyName)
        {
            return _loadedAssemblies.ContainsKey(assemblyName);
        }

        private Assembly LoadFromAssemblyPath(string assemblyPath)
        {
            return _loadedAssemblies.GetOrAdd(Path.GetFileNameWithoutExtension(assemblyPath),
                new Lazy<Assembly>(() =>
                {
                    return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                })).Value;
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
                        if (!File.Exists(binaryPath) || File.GetLastWriteTimeUtc(assetPath) > File.GetLastWriteTimeUtc(binaryPath))
                        {
                            Directory.CreateDirectory(binaryFolderPath);
                            File.Copy(assetPath, binaryPath, true);
                        }
                    }
                }
            }
        }

        private void PopulateProbingFolder(string assetPath, string relativeFolderPath = null)
        {
            PopulateBinaryFolder(_probingFolderPath, assetPath, relativeFolderPath);
        }

        private void PopulateRuntimeFolder(string assetPath, string relativeFolderPath = null)
        {
            var runtimeDirectory = Path.GetDirectoryName(CSharpExtensionCompiler.EntryAssembly.Location);
            PopulateBinaryFolder(runtimeDirectory, assetPath, relativeFolderPath);
        }
    }
}
