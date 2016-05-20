using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.DotNet.Cli.Compiler.Common;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;

namespace Microsoft.DotNet.Tools.Compiler.Csc
{
    public class CompileCscCommand
    {
        private const int ExitFailed = 1;

        public static int Run(string[] args)
        {
            DebugHelper.HandleDebugSwitch(ref args);

            CommandLineApplication app = new CommandLineApplication();
            app.Name = "Orchard compile-csc";
            app.FullName = "Orchard .NET C# Compiler";
            app.Description = "Orchard C# Compiler for the .NET Platform";
            app.HandleResponseFiles = true;
            app.HelpOption("-h|--help");

            CommonCompilerOptionsCommandLine commonCompilerCommandLine = CommonCompilerOptionsCommandLine.AddOptions(app);
            AssemblyInfoOptionsCommandLine assemblyInfoCommandLine = AssemblyInfoOptionsCommandLine.AddOptions(app);

            CommandOption tempOutput = app.Option("--temp-output <arg>", "Compilation temporary directory", CommandOptionType.SingleValue);
            CommandOption outputName = app.Option("--out <arg>", "Name of the output assembly", CommandOptionType.SingleValue);
            CommandOption references = app.Option("--reference <arg>...", "Path to a compiler metadata reference", CommandOptionType.MultipleValue);
            CommandOption analyzers = app.Option("--analyzer <arg>...", "Path to an analyzer assembly", CommandOptionType.MultipleValue);
            CommandArgument sources = app.Argument("<source-files>...", "Compilation sources", multipleValues: true);

            app.OnExecute(() =>
            {
                if (!tempOutput.HasValue())
                {
                    Reporter.Error.WriteLine("Option '--temp-output' is required");
                    return ExitFailed;
                }

                CommonCompilerOptions commonOptions = commonCompilerCommandLine.GetOptionValues();

                AssemblyInfoOptions assemblyInfoOptions = assemblyInfoCommandLine.GetOptionValues();

                var translated = TranslateCommonOptions(commonOptions, outputName.Value());

                var allArgs = new List<string>(translated);
                allArgs.AddRange(GetDefaultOptions());

                // Generate assembly info
                var assemblyInfo = Path.Combine(tempOutput.Value(), $"dotnet-compile.assemblyinfo.cs");

                File.WriteAllText(assemblyInfo, AssemblyInfoFileGenerator.GenerateCSharp(assemblyInfoOptions, sources.Values));

                allArgs.Add($"\"{assemblyInfo}\"");

                if (outputName.HasValue())
                {
                    allArgs.Add($"-out:\"{outputName.Value()}\"");
                }

                allArgs.AddRange(analyzers.Values.Select(a => $"-a:\"{a}\""));
                allArgs.AddRange(references.Values.Select(r => $"-r:\"{r}\""));
                allArgs.AddRange(sources.Values.Select(s => $"\"{s}\""));

                var rsp = Path.Combine(tempOutput.Value(), "dotnet-compile-csc.rsp");

                File.WriteAllLines(rsp, allArgs, Encoding.UTF8);

                // Execute CSC!
                var result = RunCsc(new string[] { $"-noconfig", "@" + $"{rsp}" })
                    .WorkingDirectory(Directory.GetCurrentDirectory())
                    .ForwardStdErr()
                    .ForwardStdOut()
                    .Execute();

                return result.ExitCode;
            });

            try
            {
                return app.Execute(args);
            }
            catch (Exception ex)
            {
#if DEBUG
                Reporter.Error.WriteLine(ex.ToString());
#else
                Reporter.Error.WriteLine(ex.Message);
#endif
                return ExitFailed;
            }
        }

        // TODO: Review if this is the place for default options
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
                // public sign, unless the public sign switch has explicitly been
                // set to false
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
            // Locate CoreRun
            return Command.Create("csc.dll", cscArgs);
        }
    }
}