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
using Microsoft.DotNet.ProjectModel.Files;
using Microsoft.DotNet.ProjectModel.Graph;
using Microsoft.Extensions.DependencyModel;
using NuGet.Frameworks;

namespace Orchard.Environment.Extensions.Compilers
{
    public class CSharpExtensionCompiler
    {
        private static string RefsDirectoryName = "refs";
        private static readonly ConcurrentDictionary<string, bool> _ambientLibraries = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, bool> _compiledLibraries = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private static readonly Lazy<Assembly> _entryAssembly = new Lazy<Assembly>(Assembly.GetEntryAssembly);

        public CSharpExtensionCompiler ()
        {
            Diagnostics = new List<string>();
        }

        public static Assembly EntryAssembly => _entryAssembly.Value;
        public IList<string> Diagnostics { get; private set; }

        public bool Compile(ProjectContext context, string config, string probingFolderPath)
        {
            // Mark ambient libraries as compiled
            if (_ambientLibraries.IsEmpty)
            {
                var libraries = DependencyContext.Default.CompileLibraries
                    .Where(x => x.Type == LibraryType.Project.ToString().ToLowerInvariant());

                foreach (var library in libraries)
                {
                    _ambientLibraries[library.Name] = true;
                    _compiledLibraries[library.Name] = true;
                }
            }

            bool compilationResult;

            // Check if already compiled
            if (_compiledLibraries.TryGetValue(context.RootProject.Identity.Name, out compilationResult))
            {
                return _compiledLibraries[context.RootProject.Identity.Name];
            }

            // Get compilation options
            var compilationOptions = context.ResolveCompilationOptions(config);
            var compileInclude = context.ResolveCompilationOptions(config).CompileInclude;

            var projectSourceFiles = new List<string>();

            // Get project source files
            if (compilationOptions.CompileInclude == null)
            {
                projectSourceFiles.AddRange(context.ProjectFile.Files.SourceFiles);
            }
            else {
                var includeFiles = IncludeFilesResolver.GetIncludeFiles(compilationOptions.CompileInclude, "/", diagnostics: null);
                projectSourceFiles.AddRange(includeFiles.Select(f => f.SourcePath));
            }

            // Check if precompiled
            if (!projectSourceFiles.Any())
            {
                return _compiledLibraries[context.RootProject.Identity.Name] = true;
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

            var diagnostics = new List<DiagnosticMessage>();

            // Collect dependency diagnostics
            foreach (var diag in context.LibraryManager.GetAllDiagnostics())
            {
                // Some library paths may not be resolved by the project model (e.g in production)
                // So, here we don't grab any diagnostics and we will search in the probing folder

                //Diagnostics.Add(diag.FormattedMessage);
                //diagnostics.Add(diag);
            }

            if (diagnostics.Any(d => d.Severity == DiagnosticMessageSeverity.Error))
            {
                // We got an unresolved dependency or missing framework. Don't continue the compilation.
                return _compiledLibraries[context.RootProject.Identity.Name] = false;
            }

            // Get compilation options
            var outputName = outputPaths.CompilationFiles.Assembly;

            // Set default platform if it isn't already set and we're on desktop
            if (compilationOptions.EmitEntryPoint == true && string.IsNullOrEmpty(compilationOptions.Platform) && context.TargetFramework.IsDesktop())
            {
                // See https://github.com/dotnet/cli/issues/2428 for more details.
                compilationOptions.Platform = RuntimeInformation.ProcessArchitecture == Architecture.X64 ?
                    "x64" : "anycpu32bitpreferred";
            }

            var references = new List<string>();
            var sourceFiles = new List<string>();

            // Add metadata options
            var assemblyInfoOptions = AssemblyInfoOptions.CreateForProject(context);

            // Get the runtime directory
            var runtimeDirectory = Path.GetDirectoryName(EntryAssembly.Location);

            foreach (var dependency in dependencies)
            {
                sourceFiles.AddRange(dependency.SourceReferences.Select(s => s.GetTransformedFile(intermediateOutputPath)));

                var library = dependency.Library as ProjectDescription;
                var package = dependency.Library as PackageDescription;

                // Compile other referenced libraries
                if (library != null  && dependency.CompilationAssemblies.Any())
                {
                    if (!_compiledLibraries.TryGetValue(library.Identity.Name, out compilationResult))
                    {
                        var projectContext = ProjectContext.CreateContextForEachFramework(library.Project.ProjectDirectory).FirstOrDefault();

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
                    var fileName = library.Identity.Name + FileNameSuffixes.DotNet.DynamicLib;

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
                    foreach (var assembly in package.CompileTimeAssemblies)
                    {
                        var fileName = Path.GetFileName(assembly.Path);

                        // Search in the runtime directory
                        var path = Path.Combine(runtimeDirectory, fileName);

                        if (!File.Exists(path))
                        {
                            // Fallback to the "refs" subfolder
                            path = Path.Combine(runtimeDirectory, RefsDirectoryName, fileName);

                            if (!File.Exists(path))
                            {
                                // Fallback to the project output path or probing folder
                                path = ResolveAssetPath(outputPath, probingFolderPath, fileName);
                            }
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
                    var projectContext = ProjectContext.CreateContextForEachFramework(library.Project.ProjectDirectory).FirstOrDefault();

                    if (projectContext != null)
                    {
                        var fileName = library.Identity.Name + FileNameSuffixes.DotNet.DynamicLib;

                        // Search in the precompiled project output path
                        var path = Path.Combine(projectContext.GetOutputPaths(config).CompilationOutputPath, fileName);

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
                }
                // Check for an ambient library
                else if (library != null && _ambientLibraries.TryGetValue(library.Identity.Name, out compilationResult))
                {
                    references.AddRange(dependency.CompilationAssemblies.Select(r => Path.Combine(runtimeDirectory, r.FileName)));
                }
                else
                {
                    references.AddRange(dependency.CompilationAssemblies.Select(r => r.ResolvedPath));
                }
            }

            // Check again if already compiled, here through the dependency graph
            if (_compiledLibraries.TryGetValue(context.RootProject.Identity.Name, out compilationResult))
            {
                return _compiledLibraries[context.RootProject.Identity.Name];
            }

            var sw = Stopwatch.StartNew();

            var resources = new List<string>();

            string depsJsonFile = null;
            DependencyContext dependencyContext = null;

            // Add dependency context as a resource
            if (compilationOptions.PreserveCompilationContext == true)
            {
                var allExports = exporter.GetAllExports().Where(x => x.Library.Compatible).ToList();
                dependencyContext = new DependencyContextBuilder().Build(compilationOptions,
                    allExports,
                    allExports,
                    false, // For now, just assume non-portable mode in the legacy deps file (this is going away soon anyway)
                    context.TargetFramework,
                    context.RuntimeIdentifier ?? string.Empty);

                depsJsonFile = Path.Combine(intermediateOutputPath, compilationOptions.OutputName + "dotnet-compile.deps.json");
                resources.Add($"\"{depsJsonFile}\",{compilationOptions.OutputName}.deps.json");
            }

            // Add project source files
            sourceFiles.AddRange(projectSourceFiles);

            if (String.IsNullOrEmpty(intermediateOutputPath))
            {
                return _compiledLibraries[context.RootProject.Identity.Name] = false;
            }

            var translated = TranslateCommonOptions(compilationOptions, outputName);

            var allArgs = new List<string>(translated);
            allArgs.AddRange(GetDefaultOptions());

            // Generate assembly info
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
            outputs.AddRange(outputPaths.CompilationFiles.All());

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

                    if (!prevInputs.Except(newInputs).Any() && ! newInputs.Except(prevInputs).Any())
                    {
                        Debug.WriteLine(String.Format($"{context.RootProject.Identity.Name}: Previously compiled, skipping dynamic compilation."));
                        return _compiledLibraries[context.RootProject.Identity.Name] = true;
                    }
                }
                else
                {
                    // Write RSP file for the next time
                    File.WriteAllLines(rsp, allArgs);

                    Debug.WriteLine(String.Format($"{context.RootProject.Identity.Name}:  Previously compiled, skipping dynamic compilation."));
                    return _compiledLibraries[context.RootProject.Identity.Name] = true;
                }
            }

            Debug.WriteLine(String.Format($"{context.RootProject.Identity.Name}: Dynamic compiling for {context.TargetFramework.DotNetFrameworkName}"));

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
            File.WriteAllText(assemblyInfo, AssemblyInfoFileGenerator.GenerateCSharp(assemblyInfoOptions, sourceFiles));
            File.WriteAllLines(rsp, allArgs);

            // Locate runtime config files
            var runtimeConfigPath = Path.Combine(runtimeDirectory, EntryAssembly.GetName().Name + FileNameSuffixes.RuntimeConfigJson);
            var cscRuntimeConfigPath =  Path.Combine(runtimeDirectory, "csc" + FileNameSuffixes.RuntimeConfigJson);

            // Automatically create the csc runtime config file
            if (File.Exists(runtimeConfigPath) && (!File.Exists(cscRuntimeConfigPath)
                || File.GetLastWriteTimeUtc(runtimeConfigPath) > File.GetLastWriteTimeUtc(cscRuntimeConfigPath)))
            {
                File.Copy(runtimeConfigPath, cscRuntimeConfigPath, true);
            }

            // Execute CSC!
            var result = Command.Create("csc.dll", new string[] { $"-noconfig", "@" + $"{rsp}" })
                .WorkingDirectory(runtimeDirectory)
                .OnErrorLine(line => Diagnostics.Add(line))
                .OnOutputLine(line => Diagnostics.Add(line))
                .Execute();

            Debug.WriteLine(String.Empty);

            if (result.ExitCode == 0 && !Diagnostics.Any())
            {
                Debug.WriteLine(String.Format($"{context.RootProject.Identity.Name}: Dynamic compilation succeeded."));
                Debug.WriteLine($"0 Warning(s)");
                Debug.WriteLine($"0 Error(s)");
            }
            else if (result.ExitCode == 0 && Diagnostics.Any())
            {
                Debug.WriteLine(String.Format($"{context.RootProject.Identity.Name}: Dynamic compilation succeeded but has warnings."));
                Debug.WriteLine($"0 Error(s)");
            }
            else
            {
                Debug.WriteLine(String.Format($"{context.RootProject.Identity.Name}: Dynamic compilation failed."));
            }

            foreach (var diagnostic in Diagnostics)
            {
                Debug.WriteLine(diagnostic);
            }

            Debug.WriteLine($"Time elapsed {sw.Elapsed}");
            Debug.WriteLine(String.Empty);

            return _compiledLibraries[context.RootProject.Identity.Name] = result.ExitCode == 0;
        }

        private string ResolveAssetPath(string binaryFolderPath, string probingFolderPath,  string assetFileName)
        {
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
            var minDateUtc = DateTime.MaxValue;

            foreach (var outputPath in outputs)
            {
                var lastWriteTimeUtc = File.GetLastWriteTimeUtc(outputPath);

                if (lastWriteTimeUtc < minDateUtc)
                {
                    minDateUtc = lastWriteTimeUtc;
                }
            }

            // Find inputs that are newer than the earliest output
            return inputs.Any(p => File.GetLastWriteTimeUtc(p) >= minDateUtc);
        }
    }
}
