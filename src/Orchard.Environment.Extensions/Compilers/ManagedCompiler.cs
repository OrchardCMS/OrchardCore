using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.DotNet.Cli.Compiler.Common;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.ProjectModel.Compilation;
using Microsoft.Extensions.DependencyModel;
using NuGet.Frameworks;

using Microsoft.DotNet.Tools.Compiler.Csc;

namespace Microsoft.DotNet.Tools.Compiler
{
    public class ManagedCompiler : Compiler
    {
        public override bool Compile(ProjectContext context, string config, string buildBasePath)
        {
            // Set up Output Paths
            var outputPaths = context.GetOutputPaths(config, buildBasePath);
            var outputPath = outputPaths.CompilationOutputPath;
            var intermediateOutputPath = outputPaths.IntermediateOutputDirectoryPath;

            Directory.CreateDirectory(outputPath);
            Directory.CreateDirectory(intermediateOutputPath);

            // Create the library exporter
            var exporter = context.CreateExporter(config, buildBasePath);

            // Gather exports for the project
            var dependencies = exporter.GetDependencies().ToList();

            Reporter.Output.WriteLine($"Compiling {context.RootProject.Identity.Name.Yellow()} for {context.TargetFramework.DotNetFrameworkName.Yellow()}");
            var sw = Stopwatch.StartNew();

            var diagnostics = new List<DiagnosticMessage>();
            var missingFrameworkDiagnostics = new List<DiagnosticMessage>();

            // Collect dependency diagnostics
            foreach (var diag in context.LibraryManager.GetAllDiagnostics())
            {
                if (diag.ErrorCode == ErrorCodes.DOTNET1011 ||
                    diag.ErrorCode == ErrorCodes.DOTNET1012)
                {
                    missingFrameworkDiagnostics.Add(diag);
                }

                diagnostics.Add(diag);
            }

            if(diagnostics.Any(d => d.Severity == DiagnosticMessageSeverity.Error))
            {
                // We got an unresolved dependency or missing framework. Don't continue the compilation.
                PrintSummary(diagnostics, sw);
                return false;
            }

            // Get compilation options
            var outputName = outputPaths.CompilationFiles.Assembly;

            // Assemble args
            var compilerArgs = new List<string>()
            {
                $"--temp-output:{intermediateOutputPath}",
                $"--out:{outputName}"
            };

            var compilationOptions = context.ResolveCompilationOptions(config);

            // Set default platform if it isn't already set and we're on desktop
            if (compilationOptions.EmitEntryPoint == true && string.IsNullOrEmpty(compilationOptions.Platform) && context.TargetFramework.IsDesktop())
            {
                // See https://github.com/dotnet/cli/issues/2428 for more details.
                compilationOptions.Platform = RuntimeInformation.ProcessArchitecture == Architecture.X64 ?
                    "x64" : "anycpu32bitpreferred";
            }

            var languageId = CompilerUtil.ResolveLanguageId(context);

            var references = new List<string>();

            // Add compilation options to the args
            compilerArgs.AddRange(compilationOptions.SerializeToArgs());

            // Add metadata options
            compilerArgs.AddRange(AssemblyInfoOptions.SerializeToArgs(AssemblyInfoOptions.CreateForProject(context)));

            foreach (var dependency in dependencies)
            {
                references.AddRange(dependency.CompilationAssemblies.Select(r => r.ResolvedPath));

                compilerArgs.AddRange(dependency.SourceReferences.Select(s => s.GetTransformedFile(intermediateOutputPath)));

                // Add analyzer references
                compilerArgs.AddRange(dependency.AnalyzerReferences
                    .Where(a => a.AnalyzerLanguage == languageId)
                    .Select(a => $"--analyzer:{a.AssemblyPath}"));
            }

            compilerArgs.AddRange(references.Select(r => $"--reference:{r}"));

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

                compilerArgs.Add($"--resource:\"{depsJsonFile}\",{compilationOptions.OutputName}.deps.json");
            }

            // Add project source files
            var sourceFiles = CompilerUtil.GetCompilationSources(context, compilationOptions);
            compilerArgs.AddRange(sourceFiles);

            var compilerName = compilationOptions.CompilerName;

            // Write RSP file
            var rsp = Path.Combine(intermediateOutputPath, $"dotnet-compile.rsp");
            File.WriteAllLines(rsp, compilerArgs);

            // Run pre-compile event
            var contextVariables = new Dictionary<string, string>()
            {
                { "compile:TargetFramework", context.TargetFramework.GetShortFolderName() },
                { "compile:FullTargetFramework", context.TargetFramework.DotNetFrameworkName },
                { "compile:Configuration", config },
                { "compile:OutputFile", outputName },
                { "compile:OutputDir", outputPath.TrimEnd('\\', '/') },
                { "compile:ResponseFile", rsp }
            };

            // Cache the reporters before invoking the command in case it is a built-in command, which replaces
            // the static Reporter instances.
            Reporter errorReporter = Reporter.Error;
            Reporter outputReporter = Reporter.Output;

            CommandResult result = new BuiltInCommand($"compile-{compilerName}", new[] { $"@{rsp}" }, CompileCscCommand.Run)
                .WorkingDirectory(context.ProjectDirectory)
                .OnErrorLine(line => HandleCompilerOutputLine(line, context, diagnostics, errorReporter))
                .OnOutputLine(line => HandleCompilerOutputLine(line, context, diagnostics, outputReporter))
                .Execute();

            // Run post-compile event
            contextVariables["compile:CompilerExitCode"] = result.ExitCode.ToString();

            var success = result.ExitCode == 0;

            if (!success)
            {
                Reporter.Error.WriteLine($"{result.StartInfo.FileName} {result.StartInfo.Arguments} returned Exit Code {result.ExitCode}");
            }

            return PrintSummary(diagnostics, sw, success);
        }

        private static void HandleCompilerOutputLine(string line, ProjectContext context, List<DiagnosticMessage> diagnostics, Reporter reporter)
        {
            var diagnostic = ParseDiagnostic(context.ProjectDirectory, line);
            if (diagnostic != null)
            {
                diagnostics.Add(diagnostic);
            }
            else
            {
                reporter.WriteLine(line);
            }
        }
    }
}
