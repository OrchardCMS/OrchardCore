using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.DotNet.Cli.Compiler.Common;
using Microsoft.DotNet.Cli.Utils;
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

        private static HashSet<string> AmbientLibraries => _ambientLibraries.Value;
        private static readonly Lazy<HashSet<string>> _ambientLibraries = new Lazy<HashSet<string>>(GetAmbientLibraries);
        private static readonly ConcurrentDictionary<string, bool> _compiledLibraries = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private static readonly Lazy<Assembly> _entryAssembly = new Lazy<Assembly>(Assembly.GetEntryAssembly);

        public CSharpExtensionCompiler ()
        {
            Diagnostics = new List<string>();
        }

        public static Assembly EntryAssembly => _entryAssembly.Value;
        public IList<string> Diagnostics { get; private set; }

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
            var runtimeDirectory = Path.GetDirectoryName(EntryAssembly.Location);

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
                    var fileName = GetAssemblyFileName(library.Identity.Name);

                    // Search in the runtime directory
                    var path = Path.Combine(runtimeDirectory, fileName);

                    if (!File.Exists(path))
                    {
                        // Fallback to the project output path or probing folder
                        path = ResolveAssetPath(outputPath, probingFolderPath, fileName);
                    }

                    if (!String.IsNullOrEmpty(path))
                    {
                        references.Add(path);
                    }
                }
                // Check for an unresolved package
                else if (package != null && !package.Resolved)
                {
                    var runtimeAssets = new HashSet<string>(package.RuntimeAssemblies.Select(x => x.Path), StringComparer.OrdinalIgnoreCase);

                    foreach (var asset in package.CompileTimeAssemblies)
                    {
                        var assetFileName = Path.GetFileName(asset.Path);
                        var isRuntimeAsset = runtimeAssets.Contains(asset.Path);

                        // Search in the runtime directory
                        var path = isRuntimeAsset ? Path.Combine(runtimeDirectory, assetFileName)
                            : Path.Combine(runtimeDirectory, CompilerUtility.RefsDirectoryName, assetFileName);

                        if (!File.Exists(path))
                        {
                            // Fallback to the project output path or probing folder
                            var relativeFolderPath = isRuntimeAsset ? String.Empty : CompilerUtility.RefsDirectoryName;
                            path = ResolveAssetPath(outputPath, probingFolderPath, assetFileName, relativeFolderPath);
                        }

                        if (!String.IsNullOrEmpty(path))
                        {
                            references.Add(path);
                        }
                    }
                }
                // Check for a precompiled library
                else if (library != null && !dependency.CompilationAssemblies.Any())
                {
                    var projectContext = GetProjectContextFromPath(library.Project.ProjectDirectory);

                    if (projectContext != null)
                    {
                        var fileName = GetAssemblyFileName(library.Identity.Name);

                        // Search in the precompiled project output path
                        var path = Path.Combine(projectContext.GetOutputPaths(config).CompilationOutputPath, fileName);

                        if (!File.Exists(path))
                        {
                            // Fallback to this project output path or probing folder
                            path = ResolveAssetPath(outputPath, probingFolderPath, fileName);
                        }

                        if (!String.IsNullOrEmpty(path))
                        {
                            references.Add(path);
                        }
                    }
                }
                // Check for a resolved but ambient library (compiled e.g by VS)
                else if (library != null && AmbientLibraries.Contains(library.Identity.Name))
                {
                    // Search in the regular project bin folder, fallback to the runtime directory
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

                depsJsonFile = Path.Combine(intermediateOutputPath, compilationOptions.OutputName + "dotnet-compile.deps.json"));
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
            var runtimeConfigPath = Path.Combine(runtimeDirectory, EntryAssembly.GetName().Name + FileNameSuffixes.RuntimeConfigJson);
            var cscRuntimeConfigPath =  Path.Combine(runtimeDirectory, "csc" + FileNameSuffixes.RuntimeConfigJson);

            // Automatically create the csc runtime config file
            if (File.Exists(runtimeConfigPath) && (!File.Exists(cscRuntimeConfigPath)
                || File.GetLastWriteTimeUtc(runtimeConfigPath) > File.GetLastWriteTimeUtc(cscRuntimeConfigPath)))
            {
                lock (_syncLock)
                {
                    File.Copy(runtimeConfigPath, cscRuntimeConfigPath, true);
                }
            }

            // Locate the csc.dll
            var cscDllPath = Path.Combine(runtimeDirectory, GetAssemblyFileName("csc"));

            // Automatically generate csc.dll
            if (!File.Exists(cscDllPath))
            {
                lock (_syncLock)
                {
                    if (!File.Exists(cscDllPath))
                    {
                        var cscLibrary = DependencyContext.Default?.RuntimeLibraries.Where(l => l.NativeLibraryGroups
                        .Any(g => g.Runtime == "any" && g.AssetPaths.Any(p => p.Contains("csc.exe")))).FirstOrDefault();

                        var cscRelativePath = cscLibrary?.NativeLibraryGroups.Where(g => g.Runtime == "any")
                            .SelectMany(g => g.AssetPaths).Where(p => p.Contains("csc.exe")).FirstOrDefault();

                        if (!String.IsNullOrEmpty(cscRelativePath))
                        {
                            var cscExePath = Path.Combine(runtimeDirectory, cscRelativePath);

                            if (!File.Exists(cscExePath))
                            {
                                var nugetPackagesRoot = context.PackagesDirectory;

                                if (!String.IsNullOrEmpty(nugetPackagesRoot))
                                {
                                    var cscPackagePath = Path.Combine(nugetPackagesRoot, cscLibrary.Name);
                                    cscExePath = Path.Combine(cscPackagePath, cscLibrary.Version, cscRelativePath);

                                    if (File.Exists(cscExePath))
                                    {
                                        File.Copy(cscExePath, cscDllPath, true);
                                    }
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

        private string GetAssemblyFileName(string assemblyName)
        {
            return assemblyName + FileNameSuffixes.DotNet.DynamicLib;
        }

        private string ResolveAssetPath(string binaryFolderPath, string probingFolderPath,  string assetFileName, string relativeFolderPath = null)
        {
            binaryFolderPath = !String.IsNullOrEmpty(relativeFolderPath)
                ? Path.Combine(binaryFolderPath, relativeFolderPath)
                : binaryFolderPath;

            probingFolderPath = !String.IsNullOrEmpty(relativeFolderPath)
                ? Path.Combine(probingFolderPath, relativeFolderPath)
                : probingFolderPath;

            var binaryPath = Path.Combine(binaryFolderPath, assetFileName);
            var probingPath = Path.Combine(probingFolderPath, assetFileName);

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
                    ? "-debug:full"
                    : "-debug:portable");
            }
            else
            {
                commonArgs.Add(options.DebugType == "portable"
                    ? "-debug:portable"
                    : "-debug:full");
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
