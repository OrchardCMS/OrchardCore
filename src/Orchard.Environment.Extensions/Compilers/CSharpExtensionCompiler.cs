using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private static ConcurrentDictionary<LibraryIdentity, bool> _compilationResults = new ConcurrentDictionary<LibraryIdentity, bool>();

        public CSharpExtensionCompiler ()
        {
            Diagnostics = new List<string>();
        }

        public IList<string> Diagnostics { get; private set; }

        public bool Compile(ProjectContext context, string config)
        {
            // Check if already compiled
            bool compilationResult = false;
            if (_compilationResults.TryGetValue(context.RootProject.Identity, out compilationResult))
            {
                return compilationResult;
            }

           // Mark ambient libraries as compiled
            if (_compilationResults.IsEmpty)
            {
                var projectContext = ProjectContext.CreateContextForEachFramework("").FirstOrDefault();
                var libraryExporter = projectContext.CreateExporter(config);
                var projectDependencies = libraryExporter.GetDependencies().ToList();

                foreach (var dependency in projectDependencies)
                {
                    var library = dependency.Library as ProjectDescription;
                    if (library != null)
                    {
                        _compilationResults[library.Identity] = true;  
                    }
                }
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
                // Ambient libraries may not be resolved by the project model (e.g in production)
                // So, we grab diagnostics only if the library is not marked as already compiled
                if (!_compilationResults.TryGetValue(diag.Source.Identity, out compilationResult))
                {
                    Diagnostics.Add(diag.FormattedMessage);
                    diagnostics.Add(diag);
                }
            }

            if (diagnostics.Any(d => d.Severity == DiagnosticMessageSeverity.Error))
            {
                // We got an unresolved dependency or missing framework. Don't continue the compilation.
                return _compilationResults[context.RootProject.Identity] = false;
            }

            // Get compilation options
            var outputName = outputPaths.CompilationFiles.Assembly;
            var compilationOptions = context.ResolveCompilationOptions(config);

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

            foreach (var dependency in dependencies)
            {
                sourceFiles.AddRange(dependency.SourceReferences.Select(s => s.GetTransformedFile(intermediateOutputPath)));

                // Compile other referenced libraries
                var library = dependency.Library as ProjectDescription;
                if (library != null)
                {
                    compilationResult = false;
                    if (!_compilationResults.TryGetValue(library.Identity, out compilationResult))
                    {
                        var projectContext = ProjectContext.CreateContextForEachFramework(library.Project.ProjectDirectory).FirstOrDefault();

                        if (projectContext != null)
                        {
                           // Right now, if !success we try to use the last build
                           compilationResult = Compile(projectContext, config);
                        }
                        _compilationResults[library.Identity] = compilationResult;  
                    }
                }

                // If an ambient Orchard library is not resolved (e.g in production)
                if (library != null && !dependency.CompilationAssemblies.Any())
                {
                    // Reference this ambient library by only using its name
                    references.Add(dependency.Library.Identity.Name + ".dll");
                }
                else
                {
                    references.AddRange(dependency.CompilationAssemblies.Select(r => r.ResolvedPath));
                }
            }

            // Check again if already compiled, here through the dependency graph
            if (_compilationResults.TryGetValue(context.RootProject.Identity, out compilationResult))
            {
                return compilationResult;
            }

            // Mark this library as compiled even if it will fail
            _compilationResults[context.RootProject.Identity] = false;

            var resources = new List<string>();
            if (compilationOptions.PreserveCompilationContext == true)
            {
                var allExports = exporter.GetAllExports().ToList();
                var dependencyContext = new DependencyContextBuilder().Build(compilationOptions,
                    allExports,
                    allExports,
                    false, // For now, just assume non-portable mode in the legacy deps file (this is going away soon anyway)
                    context.TargetFramework,
                    context.RuntimeIdentifier ?? string.Empty);

                var writer = new DependencyContextWriter();
                var depsJsonFile = Path.Combine(intermediateOutputPath, compilationOptions.OutputName + "dotnet-compile.deps.json");
                using (var fileStream = File.Create(depsJsonFile))
                {
                    writer.Write(dependencyContext, fileStream);
                }

                resources.Add($"\"{depsJsonFile}\",{compilationOptions.OutputName}.deps.json");
            }

            // Add project source files
            if (compilationOptions.CompileInclude == null)
            {
                sourceFiles.AddRange(context.ProjectFile.Files.SourceFiles);
            }
            else {
                var includeFiles = IncludeFilesResolver.GetIncludeFiles(compilationOptions.CompileInclude, "/", diagnostics: null);
                sourceFiles.AddRange(includeFiles.Select(f => f.SourcePath));
            }

            if (String.IsNullOrEmpty(intermediateOutputPath))
            {
                return false;
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
                        return _compilationResults[context.RootProject.Identity] = true;
                }
                else
                {
                    // Write RSP file for the next time
                    File.WriteAllLines(rsp, allArgs);
                    return _compilationResults[context.RootProject.Identity] = true;
                }
            }

            // Write assembly info and RSP files
            File.WriteAllText(assemblyInfo, AssemblyInfoFileGenerator.GenerateCSharp(assemblyInfoOptions, sourceFiles));
            File.WriteAllLines(rsp, allArgs);

            // Execute CSC!
            var result = RunCsc(new string[] { $"-noconfig", "@" + $"{rsp}" })
                .WorkingDirectory(Directory.GetCurrentDirectory())
                .OnErrorLine(line => Diagnostics.Add(line))
                .OnOutputLine(line => Diagnostics.Add(line))
                .Execute();

            return _compilationResults[context.RootProject.Identity] = result.ExitCode == 0;
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

        private static Command RunCsc(string[] cscArgs)
        {
            // Locate runtime config files
            var entryAssembly = Assembly.GetEntryAssembly();
            var runtimeDirectory = Path.GetDirectoryName(entryAssembly.Location);
            var runtimeConfigPath = Path.Combine(runtimeDirectory, entryAssembly.GetName().Name + ".runtimeconfig.json");
            var cscRuntimeConfigPath =  Path.Combine(runtimeDirectory, "csc.runtimeconfig.json");

            // Automatically create the csc runtime config file
            if (File.Exists(runtimeConfigPath) && (!File.Exists(cscRuntimeConfigPath)
                || File.GetLastWriteTimeUtc(runtimeConfigPath) > File.GetLastWriteTimeUtc(cscRuntimeConfigPath)))
            {
                File.Copy(runtimeConfigPath, cscRuntimeConfigPath, true);
            }

            // Locate CoreRun
            return Command.Create("csc.dll", cscArgs);
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
