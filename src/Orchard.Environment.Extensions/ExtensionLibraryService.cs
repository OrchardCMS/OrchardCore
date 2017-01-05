using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Hosting;
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
using Microsoft.Extensions.Options;
using NuGet.Packaging;
using Orchard.Environment.Extensions.Compilers;

namespace Orchard.Environment.Extensions
{
    public class ExtensionLibraryService : IExtensionLibraryService
    {
        private readonly string _probingDirectoryName;
        public static string Configuration => _configuration.Value;
        private static readonly Lazy<string> _configuration = new Lazy<string>(GetConfiguration);
        private static readonly Object _syncLock = new Object();

        private static HashSet<string> ApplicationAssemblyNames => _applicationAssemblyNames.Value;
        private static readonly Lazy<HashSet<string>> _applicationAssemblyNames = new Lazy<HashSet<string>>(GetApplicationAssemblyNames);
        private static readonly ConcurrentDictionary<string, Lazy<Assembly>> _loadedAssemblies = new ConcurrentDictionary<string, Lazy<Assembly>>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, string> _compileOnlyAssemblies = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly Lazy<List<MetadataReference>> _metadataReferences;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly string _probingFolderPath;
        private readonly ILogger _logger;

        public ExtensionLibraryService(
            IHostingEnvironment hostingEnvironment,
            IOptions<ExtensionProbingOptions> optionsAccessor,
            ILogger<ExtensionLibraryService> logger)
        {
            _metadataReferences = new Lazy<List<MetadataReference>>(GetMetadataReferences);
            _hostingEnvironment = hostingEnvironment;
            _probingDirectoryName = optionsAccessor.Value.DependencyProbingDirectoryName;
            _probingFolderPath = _hostingEnvironment.ContentRootFileProvider.GetFileInfo(Path.Combine(optionsAccessor.Value.RootProbingName, _probingDirectoryName)).PhysicalPath;
            _logger = logger;
        }

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

            foreach (var assemblyName in _compileOnlyAssemblies.Keys)
            {
                if (assemblyNames.Add(assemblyName))
                {
                    var metadataReference = MetadataReference.CreateFromFile(_compileOnlyAssemblies[assemblyName]);
                    metadataReferences.Add(metadataReference);
                }
            }

            foreach (var assemblyName in _loadedAssemblies.Keys)
            {
                if (assemblyNames.Add(assemblyName))
                {
                    var metadataReference = MetadataReference.CreateFromFile(_loadedAssemblies[assemblyName].Value.Location);
                    metadataReferences.Add(metadataReference);
                }
            }

            return metadataReferences;
        }

        public Assembly LoadAmbientExtension(IExtensionInfo extensionInfo)
        {
            if (IsAmbientExtension(extensionInfo))
            {
                return Assembly.Load(new AssemblyName(extensionInfo.Id));
            }

            return null;
        }

        public Assembly LoadPrecompiledExtension(IExtensionInfo extensionInfo)
        {
            if (IsAmbientExtension(extensionInfo))
            {
                return null;
            }

            var projectContext = GetProjectContext(extensionInfo);

            if (!IsPrecompiledContext(projectContext))
            {
                return null;
            }

            if (IsAssemblyLoaded(extensionInfo.Id))
            {
                return _loadedAssemblies[extensionInfo.Id].Value;
            }

            if (projectContext != null)
            {
                LoadProject(projectContext);
            }
            else
            {
                LoadPrecompiledModule(extensionInfo);
            }

            return IsAssemblyLoaded(extensionInfo.Id) ? _loadedAssemblies[extensionInfo.Id].Value : null;
        }

        public Assembly LoadDynamicExtension(IExtensionInfo extensionInfo)
        {
            if (IsAmbientExtension(extensionInfo))
            {
                return null;
            }

            var projectContext = GetProjectContext(extensionInfo);

            if (!IsDynamicContext(projectContext))
            {
                return null;
            }

            if (IsAssemblyLoaded(extensionInfo.Id))
            {
                return _loadedAssemblies[extensionInfo.Id].Value;
            }

            CompileProject(projectContext);
            LoadProject(projectContext);

            return IsAssemblyLoaded(extensionInfo.Id) ? _loadedAssemblies[extensionInfo.Id].Value : null;
        }

        internal ProjectContext GetProjectContext(IExtensionInfo extensionInfo)
        {
            return GetProjectContextFromPath(extensionInfo.ExtensionFileInfo.PhysicalPath);
        }

        internal ProjectContext GetProjectContextFromPath(string projectPath)
        {
            return File.Exists(Path.Combine(projectPath, Project.FileName))
                && File.Exists(Path.Combine(projectPath, LockFile.FileName))
                ? ProjectContext.CreateContextForEachFramework(projectPath).FirstOrDefault()
                : null;
        }

        internal void CompileProject(ProjectContext context)
        {

            var compiler = new CSharpExtensionCompiler(_hostingEnvironment.ApplicationName);
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

        internal void LoadProject(ProjectContext context)
        {
            var outputPaths = context.GetOutputPaths(Configuration);
            var assemblyPath = outputPaths.CompilationFiles.Assembly;
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
                        var assetFileName = CompilerUtility.GetAssemblyFileName(library.Identity.Name);
                        var assetResolvedPath = ResolveAssemblyPath(assemblyFolderPath, assetFileName);

                        if (String.IsNullOrEmpty(assetResolvedPath))
                        {
                            // Fallback to this (possible) precompiled module bin folder
                            var path = Path.Combine(Paths.GetParentFolderPath(library.Path), Constants.BinDirectoryName, assetFileName);
                            assetResolvedPath = File.Exists(path) ? path : null;
                        }

                        if (!String.IsNullOrEmpty(assetResolvedPath))
                        {
                            LoadFromAssemblyPath(assetResolvedPath);
                            PopulateBinaryFolder(assemblyFolderPath, assetResolvedPath);
                            PopulateProbingFolder(assetResolvedPath);

                            var resourceFileName = library.Identity.Name + ".resources.dll";
                            var assemblyFolderName = Paths.GetFolderName(assemblyFolderPath);

                            var assetFolderPath = Paths.GetParentFolderPath(assetResolvedPath);
                            var assetFolderName = Paths.GetFolderName(assetFolderPath);

                            var resourceAssemblies = Directory.GetFiles(assetFolderPath, resourceFileName, SearchOption.AllDirectories)
                                .Union(Directory.GetFiles(assemblyFolderPath, resourceFileName, SearchOption.AllDirectories))
                                .Union(Directory.GetFiles(_probingFolderPath, resourceFileName, SearchOption.AllDirectories));

                            foreach (var asset in resourceAssemblies)
                            {
                                var locale = Paths.GetParentFolderName(asset)
                                    .Replace(assetFolderName, String.Empty)
                                    .Replace(assemblyFolderName, String.Empty)
                                    .Replace(_probingDirectoryName, String.Empty);

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
                    string fallbackBinPath = null;

                    foreach (var asset in package.RuntimeAssemblies)
                    {
                        var assetName = Path.GetFileNameWithoutExtension(asset.Path);

                        if (!IsAmbientAssembly(assetName))
                        {
                            var assetFileName = Path.GetFileName(asset.Path);
                            var assetResolvedPath = ResolveAssemblyPath(assemblyFolderPath, assetFileName);

                            if (String.IsNullOrEmpty(assetResolvedPath))
                            {
                                if (fallbackBinPath == null)
                                {
                                    fallbackBinPath = String.Empty;

                                    // Fallback to a (possible) parent precompiled module bin folder
                                    var parentBinPaths = CompilerUtility.GetOtherParentProjectsLocations(context, package)
                                        .Select(x => Path.Combine(x, Constants.BinDirectoryName));

                                    foreach (var binaryPath in parentBinPaths)
                                    {
                                        var path = Path.Combine(binaryPath, assetFileName);

                                        if (File.Exists(path))
                                        {
                                            assetResolvedPath = path;
                                            fallbackBinPath = binaryPath;
                                            break;
                                        }
                                    }
                                }
                                else if (!String.IsNullOrEmpty(fallbackBinPath))
                                {
                                    var path = Path.Combine(fallbackBinPath, assetFileName);
                                    assetResolvedPath = File.Exists(path) ? path : null;
                                }
                            }

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
                                ? Paths.GetParentFolderPath(asset.Path) : String.Empty;

                            var assetResolvedPath = ResolveAssemblyPath(assemblyFolderPath, assetFileName, relativeFolderPath);

                            if (String.IsNullOrEmpty(assetResolvedPath) && !String.IsNullOrEmpty(fallbackBinPath))
                            {
                                var path = Path.Combine(fallbackBinPath, relativeFolderPath, assetFileName);
                                assetResolvedPath = File.Exists(path) ? path : null;
                            }

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

                            if (String.IsNullOrEmpty(assetResolvedPath) && !String.IsNullOrEmpty(fallbackBinPath))
                            {
                                var path = Path.Combine(fallbackBinPath, CompilerUtility.RefsDirectoryName, assetFileName);
                                assetResolvedPath = File.Exists(path) ? path : null;
                            }

                            if (!String.IsNullOrEmpty(assetResolvedPath))
                            {
                                _compileOnlyAssemblies[assetFileName] = assetResolvedPath;
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

                                if (String.IsNullOrEmpty(assetResolvedPath) && !String.IsNullOrEmpty(fallbackBinPath))
                                {
                                    var path = Path.Combine(fallbackBinPath, locale, assetFileName);
                                    assetResolvedPath = File.Exists(path) ? path : null;
                                }

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
                        var assetFileName = CompilerUtility.GetAssemblyFileName(library.Identity.Name);
                        var assetResolvedPath = ResolveAssemblyPath(assemblyFolderPath, assetFileName);

                        if (String.IsNullOrEmpty(assetResolvedPath))
                        {
                            // Fallback to this precompiled project output path
                            var outputPath = CompilerUtility.GetAssemblyFolderPath(library.Project.ProjectDirectory,
                                Configuration, context.TargetFramework.DotNetFrameworkName);
                            var path = Path.Combine(outputPath, assetFileName);
                            assetResolvedPath = File.Exists(path) ? path : null;
                        }

                        if (!String.IsNullOrEmpty(assetResolvedPath))
                        {
                            LoadFromAssemblyPath(assetResolvedPath);
                            PopulateBinaryFolder(assemblyFolderPath, assetResolvedPath);
                            PopulateProbingFolder(assetResolvedPath);

                            var resourceFileName = library.Identity.Name + ".resources.dll";
                            var assemblyFolderName = Paths.GetFolderName(assemblyFolderPath);

                            var assetFolderPath = Paths.GetParentFolderPath(assetResolvedPath);
                            var assetFolderName = Paths.GetFolderName(assetFolderPath);

                            var resourceAssemblies = Directory.GetFiles(assetFolderPath, resourceFileName, SearchOption.AllDirectories)
                                .Union(Directory.GetFiles(assemblyFolderPath, resourceFileName, SearchOption.AllDirectories))
                                .Union(Directory.GetFiles(_probingFolderPath, resourceFileName, SearchOption.AllDirectories));

                            foreach (var asset in resourceAssemblies)
                            {
                                var locale = Paths.GetParentFolderName(asset)
                                    .Replace(assetFolderName, String.Empty)
                                    .Replace(assemblyFolderName, String.Empty)
                                    .Replace(_probingDirectoryName, String.Empty);

                                PopulateBinaryFolder(assemblyFolderPath, asset, locale);
                                PopulateProbingFolder(asset, locale);
                                PopulateRuntimeFolder(asset, locale);
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
                                    ? Paths.GetParentFolderPath(asset.RelativePath) : String.Empty;

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
                            _compileOnlyAssemblies[asset.Name] = asset.ResolvedPath;
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
        }

        internal void LoadPrecompiledModule(IExtensionInfo extensionInfo)
        {
            var fileInfo = extensionInfo.ExtensionFileInfo;
            var assemblyFolderPath = Path.Combine(fileInfo.PhysicalPath, Constants.BinDirectoryName);
            var assemblyPath = Path.Combine(assemblyFolderPath, CompilerUtility.GetAssemblyFileName(extensionInfo.Id));

            if (!Directory.Exists(assemblyFolderPath))
            {
                return;
            }

            // default runtime assemblies: "bin/{assembly}.dll"
            var runtimeAssemblies = Directory.GetFiles(assemblyFolderPath,
                "*" + FileNameSuffixes.DotNet.DynamicLib, SearchOption.TopDirectoryOnly);

            foreach (var asset in runtimeAssemblies)
            {
                var assetName = Path.GetFileNameWithoutExtension(asset);

                if (!IsAmbientAssembly(assetName))
                {
                    var assetFileName = Path.GetFileName(asset);
                    var assetResolvedPath = ResolveAssemblyPath(assemblyFolderPath, assetFileName);
                    LoadFromAssemblyPath(assetResolvedPath);

                    PopulateBinaryFolder(assemblyFolderPath, assetResolvedPath);
                    PopulateProbingFolder(assetResolvedPath);
                }
            }

            // compile only assemblies: "bin/refs/{assembly}.dll"
            if (Directory.Exists(Path.Combine(assemblyFolderPath, CompilerUtility.RefsDirectoryName)))
            {
                var compilationAssemblies = Directory.GetFiles(
                    Path.Combine(assemblyFolderPath, CompilerUtility.RefsDirectoryName),
                    "*" + FileNameSuffixes.DotNet.DynamicLib, SearchOption.TopDirectoryOnly);

                foreach (var asset in compilationAssemblies)
                {
                    var assetFileName = Path.GetFileName(asset);
                    var assetResolvedPath = ResolveAssemblyPath(assemblyFolderPath, assetFileName, CompilerUtility.RefsDirectoryName);

                    _compileOnlyAssemblies[assetFileName] = assetResolvedPath;
                    PopulateBinaryFolder(assemblyFolderPath, assetResolvedPath, CompilerUtility.RefsDirectoryName);
                    PopulateProbingFolder(assetResolvedPath, CompilerUtility.RefsDirectoryName);
                }
            }

            // specific runtime assemblies: "bin/runtimes/{rid}/lib/{tfm}/{assembly}.dll"
            if (Directory.Exists(Path.Combine(assemblyFolderPath, PackagingConstants.Folders.Runtimes)))
            {
                var runtimeIds = GetRuntimeIdentifiers();

                var runtimeTargets = Directory.GetFiles(
                    Path.Combine(assemblyFolderPath, PackagingConstants.Folders.Runtimes),
                    "*" + FileNameSuffixes.DotNet.DynamicLib, SearchOption.AllDirectories);

                foreach (var asset in runtimeTargets)
                {
                    var assetName = Path.GetFileNameWithoutExtension(asset);

                    if (!IsAmbientAssembly(assetName))
                    {
                        var tfmPath = Paths.GetParentFolderPath(asset);
                        var libPath = Paths.GetParentFolderPath(tfmPath);
                        var lib = Paths.GetFolderName(libPath);

                        if (String.Equals(lib, PackagingConstants.Folders.Lib, StringComparison.OrdinalIgnoreCase))
                        {
                            var tfm = Paths.GetFolderName(tfmPath);
                            var runtime = Paths.GetParentFolderName(libPath);

                            var relativeFolderPath = Path.Combine(PackagingConstants.Folders.Runtimes, runtime, lib, tfm);

                            if (runtimeIds.Contains(runtime))
                            {
                                LoadFromAssemblyPath(asset);
                            }

                            PopulateProbingFolder(asset, relativeFolderPath);
                        }
                    }
                }
            }

            // resource assemblies: "bin/{locale?}/{assembly}.resources.dll"
            var resourceAssemblies = Directory.GetFiles(assemblyFolderPath, "*.resources"
                + FileNameSuffixes.DotNet.DynamicLib, SearchOption.AllDirectories);

            var assemblyFolderName = Paths.GetFolderName(assemblyFolderPath);

            foreach (var asset in resourceAssemblies)
            {
                var assetFileName = Path.GetFileName(asset);
                var locale = Paths.GetParentFolderName(asset).Replace(assemblyFolderName, String.Empty);
                var assetResolvedPath = ResolveAssemblyPath(assemblyFolderPath, assetFileName, locale);

                PopulateBinaryFolder(assemblyFolderPath, assetResolvedPath, locale);
                PopulateProbingFolder(assetResolvedPath, locale);
                PopulateRuntimeFolder(assetResolvedPath, locale);
            }
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

        private bool IsAmbientExtension(IExtensionInfo extensionInfo)
        {
            return IsAmbientAssembly(extensionInfo.Id);
        }

        private bool IsDynamicContext(ProjectContext context)
        {
            if (context == null)
            {
                return false;
            }

            var compilationOptions = context.ResolveCompilationOptions(Configuration);
            return CompilerUtility.GetCompilationSources(context, compilationOptions).Any();
        }

        private bool IsPrecompiledContext(ProjectContext context)
        {
            return context == null || !IsDynamicContext(context);
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

        private string ResolveAssemblyPath(string binaryFolderPath, string assemblyName, string relativeFolderPath = null)
        {
            return CompilerUtility.ResolveAssetPath(binaryFolderPath, _probingFolderPath, assemblyName, relativeFolderPath);
        }

        private void PopulateBinaryFolder(string binaryFolderPath, string assetPath, string relativeFolderPath = null)
        {
            if (!PathUtility.IsChildOfDirectory(binaryFolderPath, assetPath))
            {
                binaryFolderPath = !String.IsNullOrEmpty(relativeFolderPath)
                    ? Path.Combine(binaryFolderPath, relativeFolderPath)
                    : binaryFolderPath;

                var binaryPath = Path.Combine(binaryFolderPath, Path.GetFileName(assetPath));

                if (Files.IsNewer(assetPath, binaryPath))
                {
                    lock (_syncLock)
                    {
                        if (Files.IsNewer(assetPath, binaryPath))
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
            PopulateBinaryFolder(ApplicationEnvironment.ApplicationBasePath, assetPath, relativeFolderPath);
        }
    }
}
