using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.DotNet.Cli.Compiler.Common;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.InternalAbstractions;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.ProjectModel.Compilation;
using Microsoft.DotNet.ProjectModel.Graph;
using Microsoft.DotNet.ProjectModel.Resources;
using Microsoft.Extensions.DependencyModel;
using NuGet.Frameworks;

namespace Orchard.Environment.Extensions.Compilers
{
    public class CSharpExtensionCompiler
    {
        private static readonly Object _syncLock = new Object();
        private static readonly ConcurrentDictionary<string, object> _compilationlocks = new ConcurrentDictionary<string, object>();

        private static RuntimeLibrary CscLibrary => _cscLibrary.Value;
        private static RuntimeLibrary NativePDBWriter => _nativePDBWriter.Value;
        private static HashSet<string> AmbientLibraries => _ambientLibraries.Value;
        private static readonly Lazy<RuntimeLibrary> _cscLibrary = new Lazy<RuntimeLibrary>(GetCscLibrary);
        private static readonly Lazy<RuntimeLibrary> _nativePDBWriter = new Lazy<RuntimeLibrary>(GetNativePDBWriter);
        private static readonly Lazy<HashSet<string>> _ambientLibraries = new Lazy<HashSet<string>>(GetAmbientLibraries);
        private static readonly ConcurrentDictionary<string, bool> _compiledLibraries = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private readonly string _applicationName;

        public CSharpExtensionCompiler(string applicationName)
        {
            _applicationName = applicationName;
            Diagnostics = new List<string>();
        }

        public IList<string> Diagnostics { get; private set; }

        private static RuntimeLibrary GetCscLibrary()
        {
            return DependencyContext.Default?.RuntimeLibraries.FirstOrDefault(l => l.NativeLibraryGroups.Any(
                g => g.Runtime.Equals("any", StringComparison.OrdinalIgnoreCase) && g.AssetPaths.Any(
                    p => p.IndexOf("csc.exe", StringComparison.OrdinalIgnoreCase) >= 0)));
        }

        private static RuntimeLibrary GetNativePDBWriter()
        {
            return DependencyContext.Default?.RuntimeLibraries.FirstOrDefault(l => l.Name.Equals(
                "Microsoft.DiaSymReader.Native", StringComparison.OrdinalIgnoreCase));
        }

        private static HashSet<string> GetAmbientLibraries()
        {
            return new HashSet<string>(DependencyContext.Default.CompileLibraries
                .Where(x => x.Type.Equals(LibraryType.Project.ToString(), StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Name), StringComparer.OrdinalIgnoreCase);
        }

        public bool Compile(ProjectContext context, string config, string probingFolderPath)
        {
            var compilationlock = _compilationlocks.GetOrAdd(context.ProjectName(), id => new object());

            lock (compilationlock)
            {
                return CompileInternal(context, config, probingFolderPath);
            }
        }

        internal bool CompileInternal(ProjectContext context, string config, string probingFolderPath)
        {
            // Check if ambient library
            if (AmbientLibraries.Contains(context.ProjectName()))
            {
                return true;
            }

            bool compilationResult;

            // Check if already compiled
            if (_compiledLibraries.TryGetValue(context.ProjectName(), out compilationResult))
            {
                return compilationResult;
            }

            // Get compilation options and source files
            var compilationOptions = context.ResolveCompilationOptions(config);
            var projectSourceFiles = CompilerUtility.GetCompilationSources(context, compilationOptions);

            // Check if precompiled
            if (!projectSourceFiles.Any())
            {
                return _compiledLibraries[context.ProjectName()] = true;
            }

            // Set up Output Paths
            var outputPaths = context.GetOutputPaths(config);
            var outputPath = outputPaths.CompilationOutputPath;
            var intermediateOutputPath = outputPaths.IntermediateOutputDirectoryPath;

            Directory.CreateDirectory(outputPath);
            Directory.CreateDirectory(intermediateOutputPath);

            // Create the library exporter
            var exporter = context.CreateExporter(config);

            // Gather exports for the project
            var dependencies = exporter.GetDependencies().ToList();

            // Get compilation options
            var outputName = outputPaths.CompilationFiles.Assembly;

            // Set default platform if it isn't already set and we're on desktop
            if (compilationOptions.EmitEntryPoint == true && String.IsNullOrEmpty(compilationOptions.Platform) && context.TargetFramework.IsDesktop())
            {
                // See https://github.com/dotnet/cli/issues/2428 for more details.
                compilationOptions.Platform = RuntimeInformation.ProcessArchitecture == Architecture.X64 ?
                    "x64" : "anycpu32bitpreferred";
            }

            var references = new List<string>();
            var sourceFiles = new List<string>();
            var resources = new List<string>();

            // Get the runtime directory
            var runtimeDirectory = ApplicationEnvironment.ApplicationBasePath;

            foreach (var dependency in dependencies)
            {
                sourceFiles.AddRange(dependency.SourceReferences.Select(s => s.GetTransformedFile(intermediateOutputPath)));

                foreach (var resourceFile in dependency.EmbeddedResources)
                {
                    var transformedResource = resourceFile.GetTransformedFile(intermediateOutputPath);
                    var resourceName = ResourceManifestName.CreateManifestName(
                        Path.GetFileName(resourceFile.ResolvedPath), compilationOptions.OutputName);
                    resources.Add($"\"{transformedResource}\",{resourceName}");
                }

                var library = dependency.Library as ProjectDescription;
                var package = dependency.Library as PackageDescription;

                // Compile other referenced libraries
                if (library != null && !AmbientLibraries.Contains(library.Identity.Name) && dependency.CompilationAssemblies.Any())
                {
                    if (!_compiledLibraries.ContainsKey(library.Identity.Name))
                    {
                        var projectContext = GetProjectContextFromPath(library.Project.ProjectDirectory);

                        if (projectContext != null)
                        {
                            // Right now, if !success we try to use the last build
                            var success = Compile(projectContext, config, probingFolderPath);
                        }
                    }
                }

                // Check for an unresolved library
                if (library != null && !library.Resolved)
                {
                    var assetFileName = CompilerUtility.GetAssemblyFileName(library.Identity.Name);

                    // Search in the runtime directory
                    var assetResolvedPath = Path.Combine(runtimeDirectory, assetFileName);

                    if (!File.Exists(assetResolvedPath))
                    {
                        // Fallback to the project output path or probing folder
                        assetResolvedPath = ResolveAssetPath(outputPath, probingFolderPath, assetFileName);

                        if (String.IsNullOrEmpty(assetResolvedPath))
                        {
                            // Fallback to this (possible) precompiled module bin folder
                            var path = Path.Combine(Paths.GetParentFolderPath(library.Path), Constants.BinDirectoryName, assetFileName);
                            assetResolvedPath = File.Exists(path) ? path : null;
                        }
                    }

                    if (!String.IsNullOrEmpty(assetResolvedPath))
                    {
                        references.Add(assetResolvedPath);
                    }
                }
                // Check for an unresolved package
                else if (package != null && !package.Resolved)
                {
                    var runtimeAssets = new HashSet<string>(package.RuntimeAssemblies.Select(x => x.Path), StringComparer.OrdinalIgnoreCase);
                    string fallbackBinPath = null;

                    foreach (var asset in package.CompileTimeAssemblies)
                    {
                        var assetFileName = Path.GetFileName(asset.Path);
                        var isRuntimeAsset = runtimeAssets.Contains(asset.Path);

                        // Search in the runtime directory
                        var relativeFolderPath = isRuntimeAsset ? String.Empty : CompilerUtility.RefsDirectoryName;
                        var assetResolvedPath = Path.Combine(runtimeDirectory, relativeFolderPath, assetFileName);

                        if (!File.Exists(assetResolvedPath))
                        {
                            // Fallback to the project output path or probing folder
                            assetResolvedPath = ResolveAssetPath(outputPath, probingFolderPath, assetFileName, relativeFolderPath);
                        }

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
                                    var path = Path.Combine(binaryPath, relativeFolderPath, assetFileName);

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
                                var path = Path.Combine(fallbackBinPath, relativeFolderPath, assetFileName);
                                assetResolvedPath = File.Exists(path) ? path : null;
                            }
                        }

                        if (!String.IsNullOrEmpty(assetResolvedPath))
                        {
                            references.Add(assetResolvedPath);
                        }
                    }
                }
                // Check for a precompiled library
                else if (library != null && !dependency.CompilationAssemblies.Any())
                {
                    // Search in the project output path or probing folder
                    var assetFileName = CompilerUtility.GetAssemblyFileName(library.Identity.Name);
                    var assetResolvedPath = ResolveAssetPath(outputPath, probingFolderPath, assetFileName);

                    if (String.IsNullOrEmpty(assetResolvedPath))
                    {
                        // Fallback to this precompiled project output path
                        var path = Path.Combine(CompilerUtility.GetAssemblyFolderPath(library.Project.ProjectDirectory,
                            config, context.TargetFramework.DotNetFrameworkName), assetFileName);
                        assetResolvedPath = File.Exists(path) ? path : null;
                    }

                    if (!String.IsNullOrEmpty(assetResolvedPath))
                    {
                        references.Add(assetResolvedPath);
                    }
                }
                // Check for a resolved but ambient library (compiled e.g by VS)
                else if (library != null && AmbientLibraries.Contains(library.Identity.Name))
                {
                    // Search in the regular project output path, fallback to the runtime directory
                    references.AddRange(dependency.CompilationAssemblies.Select(r => File.Exists(r.ResolvedPath)
                        ? r.ResolvedPath : Path.Combine(runtimeDirectory, r.FileName)));
                }
                else
                {
                    references.AddRange(dependency.CompilationAssemblies.Select(r => r.ResolvedPath));
                }
            }

            // Check again if already compiled, here through the dependency graph
            if (_compiledLibraries.TryGetValue(context.ProjectName(), out compilationResult))
            {
                return compilationResult;
            }

            var sw = Stopwatch.StartNew();

            string depsJsonFile = null;
            DependencyContext dependencyContext = null;

            // Add dependency context as a resource
            if (compilationOptions.PreserveCompilationContext == true)
            {
                var allExports = exporter.GetAllExports().ToList();
                var exportsLookup = allExports.ToDictionary(e => e.Library.Identity.Name);
                var buildExclusionList = context.GetTypeBuildExclusionList(exportsLookup);
                var filteredExports = allExports
                    .Where(e => e.Library.Identity.Type.Equals(LibraryType.ReferenceAssembly) ||
                        !buildExclusionList.Contains(e.Library.Identity.Name));

                dependencyContext = new DependencyContextBuilder().Build(compilationOptions,
                    filteredExports,
                    filteredExports,
                    false, // For now, just assume non-portable mode in the legacy deps file (this is going away soon anyway)
                    context.TargetFramework,
                    context.RuntimeIdentifier ?? string.Empty);

                depsJsonFile = Path.Combine(intermediateOutputPath, compilationOptions.OutputName + "dotnet-compile.deps.json");
                resources.Add($"\"{depsJsonFile}\",{compilationOptions.OutputName}.deps.json");
            }

            // Add project source files
            sourceFiles.AddRange(projectSourceFiles);

            // Add non culture resources
            var resgenFiles = CompilerUtility.GetNonCultureResources(context.ProjectFile, intermediateOutputPath, compilationOptions);
            AddNonCultureResources(resources, resgenFiles);

            var translated = TranslateCommonOptions(compilationOptions, outputName);

            var allArgs = new List<string>(translated);
            allArgs.AddRange(GetDefaultOptions());

            // Add assembly info
            var assemblyInfo = Path.Combine(intermediateOutputPath, $"dotnet-compile.assemblyinfo.cs");
            allArgs.Add($"\"{assemblyInfo}\"");

            if (!String.IsNullOrEmpty(outputName))
            {
                allArgs.Add($"-out:\"{outputName}\"");
            }

            allArgs.AddRange(references.Select(r => $"-r:\"{r}\""));
            allArgs.AddRange(resources.Select(resource => $"-resource:{resource}"));
            allArgs.AddRange(sourceFiles.Select(s => $"\"{s}\""));

            // Gather all compile IO
            var inputs = new List<string>();
            var outputs = new List<string>();

            inputs.Add(context.ProjectFile.ProjectFilePath);

            if (context.LockFile != null)
            {
                inputs.Add(context.LockFile.LockFilePath);
            }

            if (context.LockFile.ExportFile != null)
            {
                inputs.Add(context.LockFile.ExportFile.ExportFilePath);
            }

            inputs.AddRange(sourceFiles);
            inputs.AddRange(references);

            inputs.AddRange(resgenFiles.Select(x => x.InputFile));

            var cultureResgenFiles = CompilerUtility.GetCultureResources(context.ProjectFile, outputPath, compilationOptions);
            inputs.AddRange(cultureResgenFiles.SelectMany(x => x.InputFileToMetadata.Keys));

            outputs.AddRange(outputPaths.CompilationFiles.All());
            outputs.AddRange(resgenFiles.Where(x => x.OutputFile != null).Select(x => x.OutputFile));
            //outputs.AddRange(cultureResgenFiles.Where(x => x.OutputFile != null).Select(x => x.OutputFile));

            // Locate RSP file
            var rsp = Path.Combine(intermediateOutputPath, $"dotnet-compile-csc.rsp");

            // Check if there is no need to compile
            if (!CheckMissingIO(inputs, outputs) && !TimestampsChanged(inputs, outputs))
            {
                if (File.Exists(rsp))
                {
                    // Check if the compilation context has been changed
                    var prevInputs = new HashSet<string>(File.ReadAllLines(rsp));
                    var newInputs = new HashSet<string>(allArgs);

                    if (!prevInputs.Except(newInputs).Any() && !newInputs.Except(prevInputs).Any())
                    {
                        PrintMessage($"{context.ProjectName()}: Previously compiled, skipping dynamic compilation.");
                        return _compiledLibraries[context.ProjectName()] = true;
                    }
                }
                else
                {
                    // Write RSP file for the next time
                    File.WriteAllLines(rsp, allArgs);

                    PrintMessage($"{context.ProjectName()}:  Previously compiled, skipping dynamic compilation.");
                    return _compiledLibraries[context.ProjectName()] = true;
                }
            }

            if (!CompilerUtility.GenerateNonCultureResources(context.ProjectFile, resgenFiles, Diagnostics))
            {
                return _compiledLibraries[context.ProjectName()] = false;
            }

            PrintMessage(String.Format($"{context.ProjectName()}: Dynamic compiling for {context.TargetFramework.DotNetFrameworkName}"));

            // Write the dependencies file
            if (dependencyContext != null)
            {
                var writer = new DependencyContextWriter();
                using (var fileStream = File.Create(depsJsonFile))
                {
                    writer.Write(dependencyContext, fileStream);
                }
            }

            // Write assembly info and RSP files
            var assemblyInfoOptions = AssemblyInfoOptions.CreateForProject(context);
            File.WriteAllText(assemblyInfo, AssemblyInfoFileGenerator.GenerateCSharp(assemblyInfoOptions, sourceFiles));
            File.WriteAllLines(rsp, allArgs);

            // Locate runtime config files
            var runtimeConfigPath = Path.Combine(runtimeDirectory, _applicationName + FileNameSuffixes.RuntimeConfigJson);
            var cscRuntimeConfigPath = Path.Combine(runtimeDirectory, "csc" + FileNameSuffixes.RuntimeConfigJson);

            // Automatically create the csc runtime config file
            if (Files.IsNewer(runtimeConfigPath, cscRuntimeConfigPath))
            {
                lock (_syncLock)
                {
                    if (Files.IsNewer(runtimeConfigPath, cscRuntimeConfigPath))
                    {
                        File.Copy(runtimeConfigPath, cscRuntimeConfigPath, true);
                    }
                }
            }

            // Locate csc.dll and the csc.exe asset
            var cscDllPath = Path.Combine(runtimeDirectory, CompilerUtility.GetAssemblyFileName("csc"));

            // Search in the runtime directory
            var cscRelativePath = Path.Combine("runtimes", "any", "native", "csc.exe");
            var cscExePath = Path.Combine(runtimeDirectory, cscRelativePath);

            // Fallback to the packages storage
            if (!File.Exists(cscExePath) && !String.IsNullOrEmpty(context.PackagesDirectory))
            {
                cscExePath = Path.Combine(context.PackagesDirectory, CscLibrary?.Name ?? String.Empty,
                    CscLibrary?.Version ?? String.Empty, cscRelativePath);
            }

            // Automatically create csc.dll
            if (Files.IsNewer(cscExePath, cscDllPath))
            {
                lock (_syncLock)
                {
                    if (Files.IsNewer(cscExePath, cscDllPath))
                    {
                        File.Copy(cscExePath, cscDllPath, true);
                    }
                }
            }

            // Locate the csc dependencies file
            var cscDepsPath = Path.Combine(runtimeDirectory, "csc.deps.json");

            // Automatically create csc.deps.json
            if (NativePDBWriter != null && Files.IsNewer(cscDllPath, cscDepsPath))
            {
                lock (_syncLock)
                {
                    if (Files.IsNewer(cscDllPath, cscDepsPath))
                    {
                        // Only reference windows native pdb writers
                        var runtimeLibraries = new List<RuntimeLibrary>();
                        runtimeLibraries.Add(NativePDBWriter);

                        DependencyContext cscDependencyContext = new DependencyContext(
                            DependencyContext.Default.Target, CompilationOptions.Default,
                            new List<CompilationLibrary>(), runtimeLibraries,
                            new List<RuntimeFallbacks>());

                        // Write the csc.deps.json file
                        if (cscDependencyContext != null)
                        {
                            var writer = new DependencyContextWriter();
                            using (var fileStream = File.Create(cscDepsPath))
                            {
                                writer.Write(cscDependencyContext, fileStream);
                            }
                        }

                        // Windows native pdb writers are outputed on dotnet publish.
                        // But not on dotnet build during development, we do it here.

                        // Check if there is a packages storage
                        if (!String.IsNullOrEmpty(context.PackagesDirectory))
                        {
                            var assetPaths = NativePDBWriter.NativeLibraryGroups.SelectMany(l => l.AssetPaths);

                            foreach (var assetPath in assetPaths)
                            {
                                // Resolve the pdb writer from the packages storage
                                var pdbResolvedPath = Path.Combine(context.PackagesDirectory,
                                    NativePDBWriter.Name, NativePDBWriter.Version, assetPath);

                                var pdbOutputPath = Path.Combine(runtimeDirectory, assetPath);

                                // Store the pdb writer in the runtime directory
                                if (Files.IsNewer(pdbResolvedPath, pdbOutputPath))
                                {
                                    Directory.CreateDirectory(Paths.GetParentFolderPath(pdbOutputPath));
                                    File.Copy(pdbResolvedPath, pdbOutputPath, true);
                                }
                            }
                        }
                    }
                }
            }

            // Execute CSC!
            var result = Command.Create("csc.dll", new string[] { $"-noconfig", "@" + $"{rsp}" })
                .WorkingDirectory(runtimeDirectory)
                .OnErrorLine(line => Diagnostics.Add(line))
                .OnOutputLine(line => Diagnostics.Add(line))
                .Execute();

            compilationResult = result.ExitCode == 0;

            if (compilationResult)
            {
                compilationResult &= CompilerUtility.GenerateCultureResourceAssemblies(context.ProjectFile, cultureResgenFiles, references, Diagnostics);
            }

            PrintMessage(String.Empty);

            if (compilationResult && Diagnostics.Count == 0)
            {
                PrintMessage($"{context.ProjectName()}: Dynamic compilation succeeded.");
                PrintMessage($"0 Warning(s)");
                PrintMessage($"0 Error(s)");
            }
            else if (compilationResult && Diagnostics.Count > 0)
            {
                PrintMessage($"{context.ProjectName()}: Dynamic compilation succeeded but has warnings.");
                PrintMessage($"0 Error(s)");
            }
            else
            {
                PrintMessage($"{context.ProjectName()}: Dynamic compilation failed.");
            }

            foreach (var diagnostic in Diagnostics)
            {
                PrintMessage(diagnostic);
            }

            PrintMessage($"Time elapsed {sw.Elapsed}");
            PrintMessage(String.Empty);

            return _compiledLibraries[context.ProjectName()] = compilationResult;
        }

        private static void PrintMessage(string message)
        {
            if (Debugger.IsAttached)
            {
                Debug.WriteLine(message);
            }
            else
            {
                Reporter.Output.WriteLine(message);
            }
        }

        private ProjectContext GetProjectContextFromPath(string projectPath)
        {
            return ProjectContext.CreateContextForEachFramework(projectPath).FirstOrDefault();
        }

        private string ResolveAssetPath(string binaryFolderPath, string probingFolderPath, string assetFileName, string relativeFolderPath = null)
        {
            return CompilerUtility.ResolveAssetPath(binaryFolderPath, probingFolderPath, assetFileName, relativeFolderPath);
        }

        private static void AddNonCultureResources(List<string> resources, List<CompilerUtility.NonCultureResgenIO> resgenFiles)
        {
            foreach (var resgenFile in resgenFiles)
            {
                if (ResourceUtility.IsResxFile(resgenFile.InputFile))
                {
                    resources.Add($"\"{resgenFile.OutputFile}\",{Path.GetFileName(resgenFile.MetadataName)}");
                }
                else
                {
                    resources.Add($"\"{resgenFile.InputFile}\",{Path.GetFileName(resgenFile.MetadataName)}");
                }
            }
        }

        private static IEnumerable<string> GetDefaultOptions()
        {
            var args = new List<string>()
            {
                "-nostdlib",
                "-nologo",
            };

            return args;
        }

        private static IEnumerable<string> TranslateCommonOptions(CommonCompilerOptions options, string outputName)
        {
            List<string> commonArgs = new List<string>();

            if (options.Defines != null)
            {
                commonArgs.AddRange(options.Defines.Select(def => $"-d:{def}"));
            }

            if (options.SuppressWarnings != null)
            {
                commonArgs.AddRange(options.SuppressWarnings.Select(w => $"-nowarn:{w}"));
            }

            // Additional arguments are added verbatim
            if (options.AdditionalArguments != null)
            {
                commonArgs.AddRange(options.AdditionalArguments);
            }

            if (options.LanguageVersion != null)
            {
                commonArgs.Add($"-langversion:{GetLanguageVersion(options.LanguageVersion)}");
            }

            if (options.Platform != null)
            {
                commonArgs.Add($"-platform:{options.Platform}");
            }

            if (options.AllowUnsafe == true)
            {
                commonArgs.Add("-unsafe");
            }

            if (options.WarningsAsErrors == true)
            {
                commonArgs.Add("-warnaserror");
            }

            if (options.Optimize == true)
            {
                commonArgs.Add("-optimize");
            }

            if (options.KeyFile != null)
            {
                commonArgs.Add($"-keyfile:\"{options.KeyFile}\"");

                // If we're not on Windows, full signing isn't supported, so we'll
                // public sign, unless the public sign switch has been set to false
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                    options.PublicSign == null)
                {
                    commonArgs.Add("-publicsign");
                }
            }

            if (options.DelaySign == true)
            {
                commonArgs.Add("-delaysign");
            }

            if (options.PublicSign == true)
            {
                commonArgs.Add("-publicsign");
            }

            if (options.GenerateXmlDocumentation == true)
            {
                commonArgs.Add($"-doc:\"{Path.ChangeExtension(outputName, "xml")}\"");
            }

            if (options.EmitEntryPoint != true)
            {
                commonArgs.Add("-t:library");
            }

            if (string.IsNullOrEmpty(options.DebugType))
            {
                commonArgs.Add(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "-debug:full" : "-debug:portable");
            }
            else
            {
                commonArgs.Add("-debug:" + options.DebugType);
            }

            return commonArgs;
        }

        private static string GetLanguageVersion(string languageVersion)
        {
            // project.json supports the enum that the roslyn APIs expose
            if (languageVersion?.StartsWith("csharp", StringComparison.OrdinalIgnoreCase) == true)
            {
                // We'll be left with the number csharp6 = 6
                return languageVersion.Substring("csharp".Length);
            }
            return languageVersion;
        }

        private bool CheckMissingIO(IEnumerable<string> inputs, IEnumerable<string> outputs)
        {
            if (!inputs.Any() || !outputs.Any())
            {
                return false;
            }

            return CheckMissingIO(inputs) || CheckMissingIO(outputs);
        }

        private bool CheckMissingIO(IEnumerable<string> items)
        {
            return items.Where(i => !File.Exists(i)).Any();
        }

        private bool TimestampsChanged(IEnumerable<string> inputs, IEnumerable<string> outputs)
        {
            // Find the output with the earliest write time
            var minDateUtc = outputs.Min(output => File.GetLastWriteTimeUtc(output));

            // Find inputs that are newer than the earliest output
            return inputs.Any(p => File.GetLastWriteTimeUtc(p) >= minDateUtc);
        }
    }
}
